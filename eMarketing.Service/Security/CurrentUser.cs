namespace eMarketing.Service.Security;

public interface ICurrentUserService
{
    CurrentUser CurrentUser { get; }
}

public sealed class CurrentUser
{
    public int? UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsAuthenticated { get; init; }
    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
    public bool IsManager => string.Equals(Role, "Yonetici", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Role, "StoreManager", StringComparison.OrdinalIgnoreCase);
    public bool IsStoreUser => string.Equals(Role, "MagazaYetkilisi", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Role, "SalesPerson", StringComparison.OrdinalIgnoreCase);
    public bool CanSeeAllStores => IsAdmin || IsManager;
}

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Yonetici = "Yonetici";
    public const string MagazaYetkilisi = "MagazaYetkilisi";
}
