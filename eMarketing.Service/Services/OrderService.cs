using System.Data;
using eMarketing.Service.Connection;
using eMarketing.Service.Dtos;
using eMarketing.Service.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace eMarketing.Service.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int? magazaId, bool tumMagazalar, CancellationToken cancellationToken = default);
    Task<OrderDetailResponseDto?> GetOrderDetailAsync(int orderId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetOrderSummaryAsync(int? magazaId, bool tumMagazalar, CancellationToken cancellationToken = default);
    Task<int> CreateOrderAsync(OrderCreateRequest request, CancellationToken cancellationToken = default);
    Task<int> CreateCartOrderAsync(CartOrderCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateOrderStatusAsync(int orderId, string status, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default);
}

public sealed class OrderService : IOrderService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreAuthorizationService _storeAuthorizationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ISqlConnectionFactory connectionFactory, ICurrentUserService currentUserService, IStoreAuthorizationService storeAuthorizationService, ILogger<OrderService> logger)
    {
        _connectionFactory = connectionFactory;
        _currentUserService = currentUserService;
        _storeAuthorizationService = storeAuthorizationService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int? magazaId, bool tumMagazalar, CancellationToken cancellationToken = default)
    {
        var orders = new List<OrderDto>();

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Siparis_Listele", connection);
        command.CommandType = CommandType.StoredProcedure;
        CurrentUser currentUser = _currentUserService.CurrentUser;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = magazaId.HasValue ? magazaId.Value : DBNull.Value;
        command.Parameters.Add("@TumMagazalar", SqlDbType.Bit).Value = tumMagazalar && currentUser.CanSeeAllStores;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = currentUser.UserId.HasValue ? currentUser.UserId.Value : DBNull.Value;
        command.Parameters.Add("@AdminMi", SqlDbType.Bit).Value = currentUser.CanSeeAllStores;

        await connection.OpenAsync(cancellationToken);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            orders.Add(new OrderDto
            {
                SiparisId = reader.GetInt("SiparisId"),
                OrderNo = reader.GetText("OrderNo"),
                CustomerId = reader.GetNullableInt("CustomerId"),
                CustomerStoreId = reader.GetNullableInt("CustomerStoreId"),
                BayiYetkiliId = reader.GetNullableInt("BayiYetkiliId"),
                MusteriAdi = reader.GetText("MusteriAdi"),
                MusteriEmail = reader.GetText("MusteriEmail"),
                MusteriTelefon = reader.GetText("MusteriTelefon"),
                MagazaAdi = reader.GetText("MagazaAdi"),
                Sehir = reader.GetText("Sehir"),
                Ilce = reader.GetText("Ilce"),
                YetkiliAdi = reader.GetText("YetkiliAdi"),
                YetkiliTelefon = reader.GetText("YetkiliTelefon"),
                YetkiliEmail = reader.GetText("YetkiliEmail"),
                GorselUrl = reader.GetText("GorselUrl"),
                UrunAdi = reader.GetText("UrunAdi"),
                Adet = reader.GetInt("Adet"),
                UrunKalemi = reader.GetInt("UrunKalemi"),
                BayiStok = reader.GetInt("BayiStok"),
                ToplamTutar = reader.GetDecimal("ToplamTutar"),
                PaymentStatus = reader.GetText("PaymentStatus"),
                OrderType = reader.GetText("OrderType"),
                OrderSource = reader.GetText("OrderSource"),
                SiparisDurumu = reader.GetText("SiparisDurumu"),
                SiparisTarihi = reader.GetNullableDate("SiparisTarihi"),
                IsCancelled = reader.GetBool("IsCancelled"),
                IsArchived = reader.GetBool("IsArchived")
            });
        }

        return orders;
    }

    public async Task<OrderDetailResponseDto?> GetOrderDetailAsync(int orderId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        OrderDto? order = await GetOrderByIdAsync(connection, orderId, cancellationToken);
        if (order == null)
            return null;

        if (!order.CustomerStoreId.HasValue || order.CustomerStoreId.Value <= 0)
            throw new UnauthorizedAccessException("Sipariş mağaza bilgisi bulunamadı.");

        await _storeAuthorizationService.EnsureStoreAccessAsync(order.CustomerStoreId.Value, cancellationToken);
        IReadOnlyList<OrderDetailItemDto> items = await GetOrderItemsAsync(connection, orderId, cancellationToken);

        return new OrderDetailResponseDto
        {
            Order = order,
            Items = items
        };
    }

    public async Task<Dictionary<string, object>> GetOrderSummaryAsync(int? magazaId, bool tumMagazalar, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Siparis_DurumOzet_Getir", connection);
        command.CommandType = CommandType.StoredProcedure;
        CurrentUser currentUser = _currentUserService.CurrentUser;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = magazaId.HasValue ? magazaId.Value : DBNull.Value;
        command.Parameters.Add("@TumMagazalar", SqlDbType.Bit).Value = tumMagazalar && currentUser.CanSeeAllStores;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = currentUser.UserId.HasValue ? currentUser.UserId.Value : DBNull.Value;
        command.Parameters.Add("@AdminMi", SqlDbType.Bit).Value = currentUser.CanSeeAllStores;

        await connection.OpenAsync(cancellationToken);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new Dictionary<string, object>();

        return new Dictionary<string, object>
        {
            ["ToplamSiparis"] = reader.GetInt("ToplamSiparis"),
            ["HazirlaniyorSayisi"] = reader.GetInt("HazirlaniyorSayisi"),
            ["KargodaSayisi"] = reader.GetInt("KargodaSayisi"),
            ["TeslimEdildiSayisi"] = reader.GetInt("TeslimEdildiSayisi"),
            ["IptalSayisi"] = reader.GetInt("IptalSayisi")
        };
    }

    public async Task<int> CreateOrderAsync(OrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.CustomerStoreId.HasValue || request.CustomerStoreId.Value <= 0)
            throw new InvalidOperationException("Sipariş için mağaza seçimi zorunludur.");

        await _storeAuthorizationService.EnsureStoreAccessAsync(request.CustomerStoreId.Value, cancellationToken);
        await _storeAuthorizationService.EnsureCanCreateOrderAsync(request.CustomerStoreId.Value, request.BayiYetkiliId, cancellationToken);

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Siparis_Ekle_TekUrun", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@CustomerName", SqlDbType.NVarChar, 300).Value = request.CustomerName.Trim();
        command.Parameters.Add("@CustomerEmail", SqlDbType.NVarChar, 400).Value = GetNullableText(request.CustomerEmail);
        command.Parameters.Add("@CustomerPhone", SqlDbType.NVarChar, 100).Value = GetNullableText(request.CustomerPhone);
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = request.ProductId;
        command.Parameters.Add("@Quantity", SqlDbType.Int).Value = request.Quantity;

        SqlParameter totalPrice = command.Parameters.Add("@TotalPrice", SqlDbType.Decimal);
        totalPrice.Precision = 18;
        totalPrice.Scale = 2;
        totalPrice.Value = request.TotalPrice;

        command.Parameters.Add("@CustomerStoreId", SqlDbType.Int).Value = request.CustomerStoreId.HasValue ? request.CustomerStoreId.Value : DBNull.Value;
        command.Parameters.Add("@OrderType", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(request.OrderType) ? "Bayi" : request.OrderType.Trim();
        command.Parameters.Add("@OrderSource", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(request.OrderSource) ? "AdminPanel" : request.OrderSource.Trim();
        command.Parameters.Add("@BayiYetkiliId", SqlDbType.Int).Value = request.BayiYetkiliId.HasValue ? request.BayiYetkiliId.Value : DBNull.Value;

        await connection.OpenAsync(cancellationToken);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        int orderId = result == null ? 0 : Convert.ToInt32(result);
        _logger.LogInformation("Order created. OrderId: {OrderId}, StoreId: {StoreId}, ProductId: {ProductId}, Quantity: {Quantity}", orderId, request.CustomerStoreId, request.ProductId, request.Quantity);
        return orderId;
    }

    public async Task<int> CreateCartOrderAsync(CartOrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.CustomerStoreId.HasValue || request.CustomerStoreId.Value <= 0)
            throw new InvalidOperationException("Sipariş için mağaza seçimi zorunludur.");

        if (request.Items.Count == 0)
            throw new InvalidOperationException("Sepette ürün bulunmalıdır.");

        await _storeAuthorizationService.EnsureStoreAccessAsync(request.CustomerStoreId.Value, cancellationToken);
        await _storeAuthorizationService.EnsureCanCreateOrderAsync(request.CustomerStoreId.Value, request.BayiYetkiliId, cancellationToken);

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            StoreCustomerInfo store = await GetStoreCustomerInfoAsync(connection, transaction, request.CustomerStoreId.Value, cancellationToken);
            await EnsureAuthorityMatchesStoreAsync(connection, transaction, request.BayiYetkiliId!.Value, store.CustomerId, request.CustomerStoreId.Value, cancellationToken);

            decimal grandTotal = request.Items.Sum(item => item.TotalPrice);
            int totalQuantity = request.Items.Sum(item => item.Quantity);
            OrderCreateItemRequest firstItem = request.Items[0];

            int orderId = await InsertOrderHeaderAsync(connection, transaction, request, store, firstItem, totalQuantity, grandTotal, cancellationToken);

            foreach (OrderCreateItemRequest item in request.Items)
                await InsertOrderItemAsync(connection, transaction, orderId, item, request.OrderSource, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Cart order created. OrderId: {OrderId}, StoreId: {StoreId}, ItemCount: {ItemCount}", orderId, request.CustomerStoreId, request.Items.Count);
            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateOrderStatusAsync(int orderId, string status, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Siparis_Durum_Guncelle", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@SiparisId", SqlDbType.Int).Value = orderId;
        command.Parameters.Add("@SiparisDurumu", SqlDbType.NVarChar, 50).Value = status.Trim();

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Order status updated. OrderId: {OrderId}, Status: {Status}", orderId, status);
    }

    public async Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Siparis_IptalEt", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@SiparisId", SqlDbType.Int).Value = orderId;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Order cancelled. OrderId: {OrderId}", orderId);
    }

    private static object GetNullableText(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
    }

    private static async Task<OrderDto?> GetOrderByIdAsync(SqlConnection connection, int orderId, CancellationToken cancellationToken)
    {
        await using SqlCommand command = new(@"
SELECT
    SiparisId,
    OrderNo,
    CustomerId,
    CustomerStoreId,
    BayiYetkiliId,
    MusteriAdi,
    MusteriEmail,
    MusteriTelefon,
    MagazaAdi,
    Sehir,
    Ilce,
    YetkiliAdi,
    YetkiliTelefon,
    YetkiliEmail,
    GorselUrl,
    UrunAdi,
    Adet,
    UrunKalemi,
    BayiStok,
    ToplamTutar,
    PaymentStatus,
    OrderType,
    OrderSource,
    SiparisDurumu,
    SiparisTarihi,
    IsCancelled,
    IsArchived
FROM dbo.vw_Siparis_Liste
WHERE SiparisId = @SiparisId
  AND IsArchived = 0;", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@SiparisId", SqlDbType.Int).Value = orderId;

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapOrder(reader);
    }

    private static async Task<IReadOnlyList<OrderDetailItemDto>> GetOrderItemsAsync(SqlConnection connection, int orderId, CancellationToken cancellationToken)
    {
        var items = new List<OrderDetailItemDto>();

        await using SqlCommand command = new(@"
SELECT
    oi.OrderItemId,
    oi.ProductId,
    p.ProductName AS UrunAdi,
    ISNULL(c.CategoryName, N'') AS KategoriAdi,
    ISNULL(p.ImageUrl, N'') AS GorselUrl,
    oi.Quantity,
    oi.UnitPrice,
    oi.LineTotal
FROM dbo.OrderItems oi
INNER JOIN dbo.Products p ON p.ProductId = oi.ProductId
LEFT JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
WHERE oi.OrderId = @SiparisId
ORDER BY oi.OrderItemId;", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@SiparisId", SqlDbType.Int).Value = orderId;

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OrderDetailItemDto
            {
                OrderItemId = reader.GetInt("OrderItemId"),
                ProductId = reader.GetInt("ProductId"),
                UrunAdi = reader.GetText("UrunAdi"),
                KategoriAdi = reader.GetText("KategoriAdi"),
                GorselUrl = reader.GetText("GorselUrl"),
                Quantity = reader.GetInt("Quantity"),
                UnitPrice = reader.GetDecimal("UnitPrice"),
                LineTotal = reader.GetDecimal("LineTotal")
            });
        }

        return items;
    }

    private static OrderDto MapOrder(SqlDataReader reader)
    {
        return new OrderDto
        {
            SiparisId = reader.GetInt("SiparisId"),
            OrderNo = reader.GetText("OrderNo"),
            CustomerId = reader.GetNullableInt("CustomerId"),
            CustomerStoreId = reader.GetNullableInt("CustomerStoreId"),
            BayiYetkiliId = reader.GetNullableInt("BayiYetkiliId"),
            MusteriAdi = reader.GetText("MusteriAdi"),
            MusteriEmail = reader.GetText("MusteriEmail"),
            MusteriTelefon = reader.GetText("MusteriTelefon"),
            MagazaAdi = reader.GetText("MagazaAdi"),
            Sehir = reader.GetText("Sehir"),
            Ilce = reader.GetText("Ilce"),
            YetkiliAdi = reader.GetText("YetkiliAdi"),
            YetkiliTelefon = reader.GetText("YetkiliTelefon"),
            YetkiliEmail = reader.GetText("YetkiliEmail"),
            GorselUrl = reader.GetText("GorselUrl"),
            UrunAdi = reader.GetText("UrunAdi"),
            Adet = reader.GetInt("Adet"),
            UrunKalemi = reader.GetInt("UrunKalemi"),
            BayiStok = reader.GetInt("BayiStok"),
            ToplamTutar = reader.GetDecimal("ToplamTutar"),
            PaymentStatus = reader.GetText("PaymentStatus"),
            OrderType = reader.GetText("OrderType"),
            OrderSource = reader.GetText("OrderSource"),
            SiparisDurumu = reader.GetText("SiparisDurumu"),
            SiparisTarihi = reader.GetNullableDate("SiparisTarihi"),
            IsCancelled = reader.GetBool("IsCancelled"),
            IsArchived = reader.GetBool("IsArchived")
        };
    }

    private static async Task<StoreCustomerInfo> GetStoreCustomerInfoAsync(SqlConnection connection, SqlTransaction transaction, int storeId, CancellationToken cancellationToken)
    {
        await using SqlCommand command = new(@"
SELECT
    c.CustomerId,
    COALESCE(NULLIF(c.CompanyName, N''), c.FullName) AS CustomerName,
    c.Email,
    COALESCE(NULLIF(cs.Phone, N''), c.Phone) AS Phone
FROM dbo.CustomerStores cs
INNER JOIN dbo.Customers c ON c.CustomerId = cs.CustomerId
WHERE cs.CustomerStoreId = @CustomerStoreId
  AND cs.IsActive = 1
  AND c.IsActive = 1;", connection, transaction);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@CustomerStoreId", SqlDbType.Int).Value = storeId;

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Seçili bayi/mağaza bulunamadı veya aktif değil.");

        return new StoreCustomerInfo(
            reader.GetInt("CustomerId"),
            reader.GetText("CustomerName"),
            reader.GetText("Email"),
            reader.GetText("Phone"));
    }

    private static async Task EnsureAuthorityMatchesStoreAsync(SqlConnection connection, SqlTransaction transaction, int authorityId, int customerId, int storeId, CancellationToken cancellationToken)
    {
        await using SqlCommand command = new(@"
SELECT COUNT(1)
FROM dbo.BayiYetkilileri byk
INNER JOIN dbo.KullaniciMagazalari km
    ON km.KullaniciId = byk.KullaniciId
   AND km.MagazaId = byk.MagazaId
   AND km.AktifMi = 1
WHERE byk.BayiYetkiliId = @BayiYetkiliId
  AND byk.BayiId = @CustomerId
  AND byk.MagazaId = @CustomerStoreId
  AND byk.AktifMi = 1
  AND byk.YetkiTipi = N'SiparisYetkilisi';", connection, transaction);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@BayiYetkiliId", SqlDbType.Int).Value = authorityId;
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        command.Parameters.Add("@CustomerStoreId", SqlDbType.Int).Value = storeId;

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        if (result == null || Convert.ToInt32(result) <= 0)
            throw new UnauthorizedAccessException("Seçili bayi yetkilisi bu bayi/mağaza için geçerli değil.");
    }

    private static async Task<int> InsertOrderHeaderAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        CartOrderCreateRequest request,
        StoreCustomerInfo store,
        OrderCreateItemRequest firstItem,
        int totalQuantity,
        decimal grandTotal,
        CancellationToken cancellationToken)
    {
        await using SqlCommand command = new(@"
INSERT INTO dbo.Orders
(
    CustomerName,
    CustomerEmail,
    CustomerPhone,
    ProductId,
    Quantity,
    TotalPrice,
    OrderStatus,
    CustomerId,
    CustomerStoreId,
    OrderNo,
    OrderType,
    OrderSource,
    PaymentStatus,
    SubTotal,
    DiscountTotal,
    ShippingTotal,
    GrandTotal,
    IsCancelled,
    IsArchived,
    BayiYetkiliId
)
VALUES
(
    @CustomerName,
    @CustomerEmail,
    @CustomerPhone,
    @ProductId,
    @Quantity,
    @TotalPrice,
    N'Hazirlaniyor',
    @CustomerId,
    @CustomerStoreId,
    NULL,
    @OrderType,
    @OrderSource,
    N'Bekliyor',
    @TotalPrice,
    0,
    0,
    @TotalPrice,
    0,
    0,
    @BayiYetkiliId
);

DECLARE @OrderId INT = SCOPE_IDENTITY();

UPDATE dbo.Orders
SET OrderNo = 'ORD-' + RIGHT('000000' + CAST(OrderId AS NVARCHAR(20)), 6)
WHERE OrderId = @OrderId;

SELECT @OrderId;", connection, transaction);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@CustomerName", SqlDbType.NVarChar, 300).Value = string.IsNullOrWhiteSpace(request.CustomerName) ? store.CustomerName : request.CustomerName.Trim();
        command.Parameters.Add("@CustomerEmail", SqlDbType.NVarChar, 400).Value = string.IsNullOrWhiteSpace(request.CustomerEmail) ? GetNullableText(store.Email) : request.CustomerEmail.Trim();
        command.Parameters.Add("@CustomerPhone", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(request.CustomerPhone) ? GetNullableText(store.Phone) : request.CustomerPhone.Trim();
        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = firstItem.ProductId;
        command.Parameters.Add("@Quantity", SqlDbType.Int).Value = totalQuantity;
        AddMoneyParameter(command, "@TotalPrice", grandTotal);
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = store.CustomerId;
        command.Parameters.Add("@CustomerStoreId", SqlDbType.Int).Value = request.CustomerStoreId!.Value;
        command.Parameters.Add("@OrderType", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(request.OrderType) ? "Bayi" : request.OrderType.Trim();
        command.Parameters.Add("@OrderSource", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(request.OrderSource) ? "Web" : request.OrderSource.Trim();
        command.Parameters.Add("@BayiYetkiliId", SqlDbType.Int).Value = request.BayiYetkiliId!.Value;

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result == null ? 0 : Convert.ToInt32(result);
    }

    private static async Task InsertOrderItemAsync(SqlConnection connection, SqlTransaction transaction, int orderId, OrderCreateItemRequest item, string orderSource, CancellationToken cancellationToken)
    {
        if (item.ProductId <= 0)
            throw new InvalidOperationException("Ürün seçimi zorunludur.");

        if (item.Quantity <= 0)
            throw new InvalidOperationException("Adet sıfırdan büyük olmalıdır.");

        await using SqlCommand stockCommand = new(@"
SELECT Stock
FROM dbo.Products
WHERE ProductId = @ProductId
  AND IsActive = 1;", connection, transaction);
        stockCommand.CommandType = CommandType.Text;
        stockCommand.Parameters.Add("@ProductId", SqlDbType.Int).Value = item.ProductId;

        object? stockResult = await stockCommand.ExecuteScalarAsync(cancellationToken);
        if (stockResult == null || stockResult == DBNull.Value)
            throw new InvalidOperationException("Ürün bulunamadı veya aktif değil.");

        int currentStock = Convert.ToInt32(stockResult);
        if (currentStock < item.Quantity)
            throw new InvalidOperationException("Seçilen ürün için yeterli stok yok.");

        decimal unitPrice = item.Quantity > 0 ? item.TotalPrice / item.Quantity : item.TotalPrice;

        await using SqlCommand itemCommand = new(@"
INSERT INTO dbo.OrderItems
(
    OrderId,
    ProductId,
    Quantity,
    UnitPrice,
    DiscountRate,
    DiscountAmount,
    LineTotal
)
VALUES
(
    @OrderId,
    @ProductId,
    @Quantity,
    @UnitPrice,
    0,
    0,
    @LineTotal
);", connection, transaction);
        itemCommand.CommandType = CommandType.Text;
        itemCommand.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
        itemCommand.Parameters.Add("@ProductId", SqlDbType.Int).Value = item.ProductId;
        itemCommand.Parameters.Add("@Quantity", SqlDbType.Int).Value = item.Quantity;
        AddMoneyParameter(itemCommand, "@UnitPrice", unitPrice);
        AddMoneyParameter(itemCommand, "@LineTotal", item.TotalPrice);
        await itemCommand.ExecuteNonQueryAsync(cancellationToken);

        await using SqlCommand updateStockCommand = new("UPDATE dbo.Products SET Stock = Stock - @Quantity WHERE ProductId = @ProductId;", connection, transaction);
        updateStockCommand.CommandType = CommandType.Text;
        updateStockCommand.Parameters.Add("@ProductId", SqlDbType.Int).Value = item.ProductId;
        updateStockCommand.Parameters.Add("@Quantity", SqlDbType.Int).Value = item.Quantity;
        await updateStockCommand.ExecuteNonQueryAsync(cancellationToken);

        await using SqlCommand movementCommand = new(@"
INSERT INTO dbo.StockMovements
(
    ProductId,
    OrderId,
    MovementType,
    Quantity,
    Description
)
VALUES
(
    @ProductId,
    @OrderId,
    N'OrderOut',
    @Quantity,
    @Description
);", connection, transaction);
        movementCommand.CommandType = CommandType.Text;
        movementCommand.Parameters.Add("@ProductId", SqlDbType.Int).Value = item.ProductId;
        movementCommand.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
        movementCommand.Parameters.Add("@Quantity", SqlDbType.Int).Value = item.Quantity;
        movementCommand.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (string.IsNullOrWhiteSpace(orderSource) ? "Web" : orderSource.Trim()) + " üzerinden sepet siparişi oluşturuldu.";
        await movementCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddMoneyParameter(SqlCommand command, string name, decimal value)
    {
        SqlParameter parameter = command.Parameters.Add(name, SqlDbType.Decimal);
        parameter.Precision = 18;
        parameter.Scale = 2;
        parameter.Value = value;
    }

    private sealed record StoreCustomerInfo(int CustomerId, string CustomerName, string Email, string Phone);

    private async Task EnsureStoreAccessAsync(SqlConnection connection, int storeId, CancellationToken cancellationToken)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        if (currentUser.CanSeeAllStores)
            return;

        if (!currentUser.UserId.HasValue)
            throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı.");

        await using SqlCommand accessCommand = new(
            "SELECT COUNT(1) FROM dbo.KullaniciMagazalari WHERE KullaniciId = @KullaniciId AND MagazaId = @MagazaId AND AktifMi = 1",
            connection);
        accessCommand.CommandType = CommandType.Text;
        accessCommand.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = currentUser.UserId.Value;
        accessCommand.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;

        object? result = await accessCommand.ExecuteScalarAsync(cancellationToken);
        if (result == null || Convert.ToInt32(result) <= 0)
            throw new UnauthorizedAccessException("Bu mağaza için sipariş oluşturma yetkiniz yok.");
    }
}
