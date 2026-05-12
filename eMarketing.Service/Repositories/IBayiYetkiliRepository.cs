using eMarketing.Service.Dtos;

namespace eMarketing.Service.Repositories;

public interface IBayiYetkiliRepository
{
    Task<IReadOnlyList<BayiYetkiliDto>> GetAsync(BayiYetkiliFilterRequest filter, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<BayiYetkiliDto?> GetByIdAsync(int id, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(BayiYetkiliSaveRequest request, CancellationToken cancellationToken = default);
    Task SetStatusAsync(int id, bool active, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SiparisYetkilisiDto>> GetOrderAuthoritiesAsync(int storeId, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default);
}
