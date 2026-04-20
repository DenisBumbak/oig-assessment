using OIG.Domain.Common;

namespace OIG.Domain.Roles;

public sealed class Role : Entity<RoleId>
{
    private readonly List<RolePermission> _permissions = new();

    public string Name { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public IReadOnlyCollection<RolePermission> Permissions => _permissions;

    private Role(RoleId id, string name, Guid? organizationId) : base(id)
    {
        Name = Normalize(name);
        OrganizationId = organizationId;
    }

    // EF
    private Role() : base(default) { Name = string.Empty; }

    public static Role Create(string name, Guid? organizationId = null)
        => new(RoleId.New(), name, organizationId);

    public void Grant(Permission permission)
    {
        if (permission == Permission.None) return;
        if (_permissions.Any(p => p.Permission == permission)) return;
        _permissions.Add(RolePermission.Create(Id, permission));
    }

    public void Rename(string name) => Name = Normalize(name);

    public void Revoke(Permission permission) => _permissions.RemoveAll(p => p.Permission == permission);

    public void ReplacePermissions(IEnumerable<Permission> permissions)
    {
        _permissions.Clear();
        foreach (var p in permissions)
            Grant(p);
    }

    private static string Normalize(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name is required.");
        if (name.Length > 100)
            throw new DomainException("Role name is too long.");
        return name;
    }
}
