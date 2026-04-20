using OIG.Domain.Roles;
using OIG.Domain.Users;

namespace OIG.Application.Abstractions.Auth;

public interface IPermissionService
{
    Task<HashSet<Permission>> GetEffectivePermissionsAsync(UserId userId, CancellationToken ct);
    Task<bool> HasPermissionAsync(UserId userId, Permission permission, CancellationToken ct);
}
