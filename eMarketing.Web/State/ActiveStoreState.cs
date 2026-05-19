using System.Globalization;
using eMarketing.Service.Dtos;
using eMarketing.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace eMarketing.Web.State;

public sealed class ActiveStoreState
{
    public const string StorageKey = "emarketing.activeStoreId";
    public const string CookieName = "emarketing.activeStoreId";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJSRuntime _jsRuntime;
    private int? _preferredStoreId;

    public List<StoreDto> Stores { get; private set; } = [];
    public StoreDto? SelectedStore { get; private set; }
    public int? SelectedStoreId => SelectedStore?.MagazaId;

    public event Action? Changed;

    public ActiveStoreState(IHttpContextAccessor httpContextAccessor, IJSRuntime jsRuntime)
    {
        _httpContextAccessor = httpContextAccessor;
        _jsRuntime = jsRuntime;
        _preferredStoreId = ReadCookieStoreId();
    }

    public async Task LoadAsync(StoreApiClient client, CancellationToken cancellationToken = default)
    {
        Stores = (await client.GetStoresAsync(cancellationToken)).ToList();
        SelectedStore = ResolveSelectedStore();
        await PersistSelectedStoreAsync();

        Changed?.Invoke();
    }

    public async Task SelectAsync(int storeId)
    {
        SelectedStore = Stores.FirstOrDefault(x => x.MagazaId == storeId) ?? SelectedStore;
        _preferredStoreId = SelectedStore?.MagazaId;
        await PersistSelectedStoreAsync();
        Changed?.Invoke();
    }

    public void Select(int storeId)
    {
        _ = SelectAsync(storeId);
    }

    public async Task ClearAsync()
    {
        Stores = [];
        SelectedStore = null;
        _preferredStoreId = null;
        await ClearPersistedStoreAsync();
        Changed?.Invoke();
    }

    public void Clear()
    {
        _ = ClearAsync();
    }

    private StoreDto? ResolveSelectedStore()
    {
        if (Stores.Count == 0)
            return null;

        int? requestedId = SelectedStore?.MagazaId ?? _preferredStoreId;
        StoreDto? requestedStore = requestedId.HasValue
            ? Stores.FirstOrDefault(x => x.MagazaId == requestedId.Value)
            : null;

        StoreDto? resolved = requestedStore ?? Stores.FirstOrDefault();
        _preferredStoreId = resolved?.MagazaId;
        return resolved;
    }

    private int? ReadCookieStoreId()
    {
        IRequestCookieCollection? cookies = _httpContextAccessor.HttpContext?.Request.Cookies;
        if (cookies == null || !cookies.TryGetValue(CookieName, out string? value))
            return null;

        return int.TryParse(value, CultureInfo.InvariantCulture, out int storeId) && storeId > 0
            ? storeId
            : null;
    }

    private async Task PersistSelectedStoreAsync()
    {
        if (SelectedStoreId is not int storeId)
            return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("eMarketing.store.save", storeId);
        }
        catch (InvalidOperationException)
        {
            // JS is not available during prerender; cookie is restored on the next interactive circuit.
        }
        catch (JSDisconnectedException)
        {
        }
    }

    private async Task ClearPersistedStoreAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("eMarketing.store.clear");
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSDisconnectedException)
        {
        }
    }
}
