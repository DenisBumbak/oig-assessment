using OIG.Domain.Organizations;

namespace OIG.Application.Abstractions.Persistence;

public interface IOrganizationRepository
{
    Task<Organization?> FindByIdAsync(OrganizationId id, CancellationToken ct);
    Task AddAsync(Organization org, CancellationToken ct);
    void Remove(Organization org);
    IQueryable<Organization> Query();
}
