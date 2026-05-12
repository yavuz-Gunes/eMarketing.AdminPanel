using System.Net.Http.Json;
using eMarketing.Service.Dtos;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public sealed class AuthApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthSession _authSession;

    public AuthApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
    {
        _httpClientFactory = httpClientFactory;
        _authSession = authSession;
    }

    public async Task LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        HttpClient client = _httpClientFactory.CreateClient("Api");
        HttpResponseMessage response = await client.PostAsJsonAsync("auth/login", new LoginRequest
        {
            KullaniciAdi = username,
            Sifre = password
        }, cancellationToken);

        LoginResponse login = await ApiClientBase.ReadRequiredAsync<LoginResponse>(response, cancellationToken);
        _authSession.Set(login);
    }
}
