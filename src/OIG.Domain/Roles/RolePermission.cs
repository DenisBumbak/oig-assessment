using OIG.Domain.Common;

namespace OIG.Domain.Roles;

public sealed class RolePermission : Entity<Guid>
{
    public RoleId RoleId { get; private set; }
    public Permission Permission { get; private set; }

    private RolePermission(Guid id, RoleId roleId, Permission permission) : base(id)
    {
        RoleId = roleId;
        Permission = permission;
    }

    private RolePermission() : base(Guid.Empty) { }

    public static RolePermission Create(RoleId roleId, Permission permission)
        => new(Guid.NewGuid(), roleId, permission);
}
