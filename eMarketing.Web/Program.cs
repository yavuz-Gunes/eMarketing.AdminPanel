using eMarketing.Web.Components;
using eMarketing.Web.Services;
using eMarketing.Web.State;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("Api", client =>
{
    string apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5088/api/";
    client.BaseAddress = new Uri(apiBaseUrl.EndsWith('/') ? apiBaseUrl : $"{apiBaseUrl}/");
});

builder.Services.AddScoped<AuthSession>();
builder.Services.AddScoped<ActiveStoreState>();
builder.Services.AddScoped<CartState>();
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<StoreApiClient>();
builder.Services.AddScoped<ProductApiClient>();
builder.Services.AddScoped<OrderApiClient>();
builder.Services.AddScoped<TeamApiClient>();
builder.Services.AddScoped<DashboardApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
