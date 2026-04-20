using OIG.Domain.Users;

namespace OIG.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(UserId id, CancellationToken ct);
    Task<User?> FindByIdWithRolesAsync(UserId id, CancellationToken ct);
    Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    void Remove(User user);
    IQueryable<User> Query();
}
