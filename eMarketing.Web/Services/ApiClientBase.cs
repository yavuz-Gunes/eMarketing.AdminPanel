using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using eMarketing.Service.Dtos;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public abstract class ApiClientBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthSession _authSession;

    protected ApiClientBase(IHttpClientFactory httpClientFactory, AuthSession authSession)
    {
        _httpClientFactory = httpClientFactory;
        _authSession = authSession;
    }

    protected HttpClient CreateClient()
    {
        HttpClient client = _httpClientFactory.CreateClient("Api");
        if (_authSession.IsAuthenticated)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authSession.Token);

        return client;
    }

    public static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("API boş yanıt döndürdü.");

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    protected static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
    }

    protected static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
            return $"API isteği başarısız oldu. HTTP {(int)response.StatusCode}";

        try
        {
            ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!string.IsNullOrWhiteSpace(error?.Message))
                return error.Message;
        }
        catch
        {
            // Fall through to plain text body.
        }

        return body.Trim('"');
    }
}
