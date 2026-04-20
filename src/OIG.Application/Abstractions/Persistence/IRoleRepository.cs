using OIG.Domain.Roles;

namespace OIG.Application.Abstractions.Persistence;

public interface IRoleRepository
{
    Task<Role?> FindByIdAsync(RoleId id, CancellationToken ct);
    Task AddAsync(Role role, CancellationToken ct);
    void Remove(Role role);
    IQueryable<Role> Query();
}
