using eMarketing.Service.Dtos;

namespace eMarketing.Service.Repositories;

public interface IPersonnelRepository
{
    Task<IReadOnlyList<PersonnelDto>> GetPersonnelAsync(PersonnelFilterRequest filter, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<int> SavePersonnelAsync(CreatePersonnelRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PersonnelStorePermissionDto>> GetStoresAsync(int userId, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PersonnelStorePermissionDto>> GetAssignableStoresAsync(int userId, string search, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task AssignStoreAsync(int userId, int storeId, string duty, CancellationToken cancellationToken = default);
    Task UpdateStoreDutyAsync(int userStoreId, string duty, CancellationToken cancellationToken = default);
    Task RemoveStoreAsync(int userStoreId, CancellationToken cancellationToken = default);
}
