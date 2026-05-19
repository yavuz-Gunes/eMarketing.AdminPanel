using System.Text;
using System.Text.Json;
using eMarketing.Api.Middleware;
using eMarketing.Api.Services;
using eMarketing.Service.Connection;
using eMarketing.Service.Dtos;
using eMarketing.Service.Repositories;
using eMarketing.Service.Security;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error => new ValidationErrorDto
            {
                Field = entry.Key,
                Message = string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Geçersiz değer." : error.ErrorMessage
            }))
            .Cast<object>()
            .ToArray();

        return new BadRequestObjectResult(new ApiErrorResponse
        {
            Message = "Validation failed.",
            TraceId = context.HttpContext.TraceIdentifier,
            Errors = errors
        });
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5090", "https://localhost:7090")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "eMarketing API",
        Version = "v1",
        Description = "Bayi, ürün, kategori ve sipariş yönetimi için okul checklist uyumlu Web API."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT token girin. Örnek: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [securityScheme] = Array.Empty<string>()
    });
});

string connectionString = builder.Configuration.GetConnectionString("DbConnection")
    ?? throw new InvalidOperationException("DbConnection connection string bulunamadı.");

builder.Services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IStoreAuthorizationService, StoreAuthorizationService>();
builder.Services.AddScoped<ISqlExecutor, SqlExecutor>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPersonnelRepository, PersonnelRepository>();
builder.Services.AddScoped<IBayiYetkiliRepository, BayiYetkiliRepository>();
builder.Services.AddScoped<IDealerRepository, DealerRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IPersonnelService, PersonnelService>();
builder.Services.AddScoped<IBayiYetkiliService, BayiYetkiliService>();
builder.Services.AddScoped<IDealerService, DealerService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IDealerOperationService, DealerOperationService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? Environment.GetEnvironmentVariable("EMARKETING_JWT_KEY")
    ?? string.Empty;

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT key bulunamadı. Development için appsettings, production için EMARKETING_JWT_KEY environment variable kullanın.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization(options =>
{
    string[] managers = { AppRoles.Admin, AppRoles.Yonetici, "StoreManager" };
    string[] storeUsers = { AppRoles.Admin, AppRoles.Yonetici, AppRoles.MagazaYetkilisi, "StoreManager", "SalesPerson", "Personel" };

    options.AddPolicy("CanViewDashboard", policy => policy.RequireRole(storeUsers));
    options.AddPolicy("CanViewOrders", policy => policy.RequireRole(storeUsers));
    options.AddPolicy("CanManageOrders", policy => policy.RequireRole(managers));
    options.AddPolicy("CanViewProducts", policy => policy.RequireRole(storeUsers));
    options.AddPolicy("CanManageProducts", policy => policy.RequireRole(managers));
    options.AddPolicy("CanViewStocks", policy => policy.RequireRole(storeUsers));
    options.AddPolicy("CanManageStock", policy => policy.RequireRole(managers));
    options.AddPolicy("CanManageStocks", policy => policy.RequireRole(managers));
    options.AddPolicy("CanManageCentralStock", policy => policy.RequireRole(managers));
    options.AddPolicy("CanViewPersonnel", policy => policy.RequireRole(storeUsers));
    options.AddPolicy("CanManagePersonnel", policy => policy.RequireRole(managers));
    options.AddPolicy("CanViewReports", policy => policy.RequireRole(managers));
    options.AddPolicy("CanManageReports", policy => policy.RequireRole(managers));
    options.AddPolicy("CanViewDealers", policy => policy.RequireRole(storeUsers));
    options.AddPolicy("CanManageDealers", policy => policy.RequireRole(managers));
    options.AddPolicy("CanManageCampaigns", policy => policy.RequireRole(managers));
    options.AddPolicy("CanManageNotifications", policy => policy.RequireRole(managers));
    options.AddPolicy("CanManageCatalog", policy => policy.RequireRole(managers));
});
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("WebClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
