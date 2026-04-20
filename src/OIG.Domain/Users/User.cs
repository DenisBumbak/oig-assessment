using OIG.Domain.Common;
using OIG.Domain.Roles;

namespace OIG.Domain.Users;

public sealed class User : Entity<UserId>
{
    private readonly List<UserRole> _userRoles = [];

    public string Name { get; private set; }
    public Email Email { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

    private User(UserId id, string name, Email email, Guid? organizationId) : base(id)
    {
        Name = NormalizeName(name);
        Email = email;
        OrganizationId = organizationId;
        IsActive = true;
    }

    private User() : base(default)
    {
        Name = string.Empty;
        Email = Email.Create("defaultmail@example.com");
    }

    public static User Register(string name, string email, Guid? organizationId = null)
        => new(UserId.New(), name, Email.Create(email), organizationId);

    public void ChangeName(string name) => Name = NormalizeName(name);
    public void ChangeEmail(string email) => Email = Email.Create(email);
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void AssignToOrganization(Guid? organizationId) => OrganizationId = organizationId;

    public void AssignRole(RoleId roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId)) return;
        _userRoles.Add(UserRole.Create(Id, roleId));
    }

    public void RemoveRole(RoleId roleId)
        => _userRoles.RemoveAll(ur => ur.RoleId == roleId);

    private static string NormalizeName(string name)
    {
        name = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("User name is required.");
        if (name.Length > 200)
            throw new DomainException("User name is too long.");
        return name;
    }
}
