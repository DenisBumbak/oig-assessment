using OIG.Domain.Users;

namespace OIG.Application.Abstractions.Auth;

public interface ICurrentUserAccessor
{
    UserId? UserId { get; }
}
