using Microsoft.EntityFrameworkCore;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Users;

namespace OIG.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> FindByIdAsync(UserId id, CancellationToken ct)
        => db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<User?> FindByIdWithRolesAsync(UserId id, CancellationToken ct)
        => db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
        => db.Users.FirstOrDefaultAsync(x => x.Email.Value == normalizedEmail, ct);

    public async Task AddAsync(User user, CancellationToken ct) => await db.Users.AddAsync(user, ct);

    public void Remove(User user) => db.Users.Remove(user);

    public IQueryable<User> Query() => db.Users.AsNoTracking();
}
