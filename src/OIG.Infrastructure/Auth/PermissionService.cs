using Microsoft.EntityFrameworkCore;
using OIG.Application.Abstractions.Auth;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Roles;
using OIG.Domain.Users;

namespace OIG.Infrastructure.Auth;

public sealed class PermissionService : IPermissionService
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IOrganizationRepository _orgs;

    public PermissionService(IUserRepository users, IRoleRepository roles, IOrganizationRepository orgs)
    {
        _users = users;
        _roles = roles;
        _orgs = orgs;
    }

    public async Task<HashSet<Permission>> GetEffectivePermissionsAsync(UserId userId, CancellationToken ct)
    {
        var user = await _users.FindByIdWithRolesAsync(userId, ct);
        if (user is null) return [];

        var assignedRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        if (assignedRoleIds.Count == 0) return [];

        var allowedOrgIds = user.OrganizationId.HasValue
            ? await CollectAncestorOrgIdsAsync(user.OrganizationId.Value, ct)
            : new HashSet<Guid>();

        var roles = await _roles.Query()
            .Include(r => r.Permissions)
            .Where(r => assignedRoleIds.Contains(r.Id))
            .ToListAsync(ct);

        var validRoles = roles.Where(r =>
            !r.OrganizationId.HasValue || allowedOrgIds.Contains(r.OrganizationId.Value));

        var permissions = new HashSet<Permission>();
        foreach (var role in validRoles)
            foreach (var rp in role.Permissions)
                permissions.Add(rp.Permission);

        return permissions;
    }

    public async Task<bool> HasPermissionAsync(UserId userId, Permission permission, CancellationToken ct)
    {
        var permissions = await GetEffectivePermissionsAsync(userId, ct);
        return permissions.Contains(permission);
    }

    private async Task<HashSet<Guid>> CollectAncestorOrgIdsAsync(Guid orgId, CancellationToken ct)
    {
        var allOrgs = await _orgs.Query()
            .Select(o => new { Id = o.Id.Value, ParentId = o.ParentId.HasValue ? o.ParentId.Value.Value : (Guid?)null })
            .ToListAsync(ct);

        var parentMap = allOrgs
            .Where(o => o.ParentId.HasValue)
            .ToDictionary(o => o.Id, o => o.ParentId!.Value);

        var result = new HashSet<Guid> { orgId };
        var current = orgId;
        while (parentMap.TryGetValue(current, out var parentId))
        {
            result.Add(parentId);
            current = parentId;
        }

        return result;
    }
}
