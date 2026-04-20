using Microsoft.EntityFrameworkCore;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Organizations;

namespace OIG.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository(AppDbContext db) : IOrganizationRepository
{
    public Task<Organization?> FindByIdAsync(OrganizationId id, CancellationToken ct)
        => db.Organizations.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Organization org, CancellationToken ct) => await db.Organizations.AddAsync(org, ct);

    public void Remove(Organization org) => db.Organizations.Remove(org);

    public IQueryable<Organization> Query() => db.Organizations.AsNoTracking();
}
