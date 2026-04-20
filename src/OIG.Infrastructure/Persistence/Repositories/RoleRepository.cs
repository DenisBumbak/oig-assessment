using Microsoft.EntityFrameworkCore;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Roles;

namespace OIG.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository(AppDbContext db) : IRoleRepository
{
    public Task<Role?> FindByIdAsync(RoleId id, CancellationToken ct)
        => db.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Role role, CancellationToken ct) => await db.Roles.AddAsync(role, ct);

    public void Remove(Role role) => db.Roles.Remove(role);

    public IQueryable<Role> Query() => db.Roles.AsNoTracking();
}
