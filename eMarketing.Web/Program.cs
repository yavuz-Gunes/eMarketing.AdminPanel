using eMarketing.Web.Components;
using eMarketing.Web.Services;
using eMarketing.Web.State;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

string dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});

builder.Services.AddHttpClient("Api", client =>
{
    string apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5088/api/";
    client.BaseAddress = new Uri(apiBaseUrl.EndsWith('/') ? apiBaseUrl : $"{apiBaseUrl}/");
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthSession>();
builder.Services.AddScoped<ActiveStoreState>();
builder.Services.AddScoped<CartState>();
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<StoreApiClient>();
builder.Services.AddScoped<DealerApiClient>();
builder.Services.AddScoped<ProductApiClient>();
builder.Services.AddScoped<StockApiClient>();
builder.Services.AddScoped<OrderApiClient>();
builder.Services.AddScoped<TeamApiClient>();
builder.Services.AddScoped<DashboardApiClient>();
builder.Services.AddScoped<CampaignApiClient>();
builder.Services.AddScoped<NotificationApiClient>();

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
