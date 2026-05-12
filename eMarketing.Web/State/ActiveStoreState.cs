using eMarketing.Service.Dtos;
using eMarketing.Web.Services;

namespace eMarketing.Web.State;

public sealed class ActiveStoreState
{
    public List<StoreDto> Stores { get; private set; } = [];
    public StoreDto? SelectedStore { get; private set; }
    public int? SelectedStoreId => SelectedStore?.MagazaId;

    public event Action? Changed;

    public async Task LoadAsync(StoreApiClient client, CancellationToken cancellationToken = default)
    {
        Stores = (await client.GetStoresAsync(cancellationToken)).ToList();
        SelectedStore ??= Stores.FirstOrDefault();
        if (SelectedStore is not null && Stores.All(x => x.MagazaId != SelectedStore.MagazaId))
            SelectedStore = Stores.FirstOrDefault();

        Changed?.Invoke();
    }

    public void Select(int storeId)
    {
        SelectedStore = Stores.FirstOrDefault(x => x.MagazaId == storeId) ?? SelectedStore;
        Changed?.Invoke();
    }

    public void Clear()
    {
        Stores = [];
        SelectedStore = null;
        Changed?.Invoke();
    }
}
