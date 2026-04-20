using MediatR;
using Microsoft.EntityFrameworkCore;
using OIG.Application.Abstractions.Persistence;

namespace OIG.Application.Features.Organizations.GetHierarchy;

public sealed class GetOrganizationHierarchyHandler : IRequestHandler<GetOrganizationHierarchyQuery, GetOrganizationHierarchyResponse>
{
    private readonly IOrganizationRepository _orgs;
    public GetOrganizationHierarchyHandler(IOrganizationRepository orgs) => _orgs = orgs;

    public async Task<GetOrganizationHierarchyResponse> Handle(GetOrganizationHierarchyQuery request, CancellationToken ct)
    {
        var items = await _orgs.Query()
            .Select(o => new FlatOrg(o.Id.Value, o.Name, o.ParentId.HasValue ? o.ParentId.Value.Value : (Guid?)null))
            .ToListAsync(ct);

        var byParent = items.Where(x => x.ParentId is not null)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        OrganizationNodeDto Build(FlatOrg x)
        {
            var children = byParent.TryGetValue(x.Id, out var list)
                ? list.Select(Build).ToList()
                : new List<OrganizationNodeDto>();

            return new OrganizationNodeDto(x.Id, x.Name, x.ParentId, children);
        }

        var roots = items.Where(x => x.ParentId is null).Select(Build).ToList();
        return new GetOrganizationHierarchyResponse(roots);
    }

    private sealed record FlatOrg(Guid Id, string Name, Guid? ParentId);
}
