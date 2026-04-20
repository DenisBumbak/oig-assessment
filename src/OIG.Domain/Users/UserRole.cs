using OIG.Domain.Common;
using OIG.Domain.Roles;

namespace OIG.Domain.Users;

public sealed class UserRole : Entity<Guid>
{
    public UserId UserId { get; private set; }
    public RoleId RoleId { get; private set; }

    private UserRole(Guid id, UserId userId, RoleId roleId) : base(id)
    {
        UserId = userId;
        RoleId = roleId;
    }

    private UserRole() : base(Guid.Empty) { }

    public static UserRole Create(UserId userId, RoleId roleId)
        => new(Guid.NewGuid(), userId, roleId);
}
