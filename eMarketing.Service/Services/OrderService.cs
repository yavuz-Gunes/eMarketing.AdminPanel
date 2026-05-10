using System.Data;
using eMarketing.Service.Connection;
using eMarketing.Service.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace eMarketing.Service.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int? magazaId, bool tumMagazalar, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetOrderSummaryAsync(int? magazaId, bool tumMagazalar, CancellationToken cancellationToken = default);
    Task<int> CreateOrderAsync(OrderCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateOrderStatusAsync(int orderId, string status, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default);
}

public sealed class OrderService : IOrderService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ISqlConnectionFactory connectionFactory, ILogger<OrderService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int? magazaId, bool tumMagazalar, CancellationToken cancellationToken = default)
    {
        var orders = new List<OrderDto>();

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Siparis_Listele", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = magazaId.HasValue ? magazaId.Value : DBNull.Value;
        command.Parameters.Add("@TumMagazalar", SqlDbType.Bit).Value = tumMagazalar;

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

    public async Task<Dictionary<string, object>> GetOrderSummaryAsync(int? magazaId, bool tumMagazalar, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Siparis_DurumOzet_Getir", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = magazaId.HasValue ? magazaId.Value : DBNull.Value;
        command.Parameters.Add("@TumMagazalar", SqlDbType.Bit).Value = tumMagazalar;

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
}
