using MediatR;
using Microsoft.EntityFrameworkCore;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Organizations;

namespace OIG.Application.Features.Organizations.Update;

public sealed class UpdateOrganizationHandler : IRequestHandler<UpdateOrganizationCommand, UpdateOrganizationResponse>
{
    private readonly IOrganizationRepository _orgs;
    private readonly IUnitOfWork _uow;

    public UpdateOrganizationHandler(IOrganizationRepository orgs, IUnitOfWork uow)
    {
        _orgs = orgs;
        _uow = uow;
    }

    public async Task<UpdateOrganizationResponse> Handle(UpdateOrganizationCommand request, CancellationToken ct)
    {
        var org = await _orgs.FindByIdAsync(new OrganizationId(request.Id), ct)
                  ?? throw new InvalidOperationException("Organization not found.");

        org.Rename(request.Name);

        if (request.ParentId != org.ParentId?.Value)
        {
            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == request.Id)
                    throw new InvalidOperationException("Organization cannot be its own parent.");

                var descendantIds = await CollectDescendantIdsAsync(request.Id, ct);
                if (descendantIds.Contains(request.ParentId.Value))
                    throw new InvalidOperationException("Cannot move organization under its own descendant.");

                var parent = await _orgs.FindByIdAsync(new OrganizationId(request.ParentId.Value), ct)
                             ?? throw new InvalidOperationException("Parent organization not found.");
            }

            org.ChangeParent(request.ParentId.HasValue ? new OrganizationId(request.ParentId.Value) : null);
        }

        await _uow.SaveChangesAsync(ct);

        return new UpdateOrganizationResponse(org.Id.Value, org.Name, org.ParentId?.Value);
    }

    private async Task<HashSet<Guid>> CollectDescendantIdsAsync(Guid orgId, CancellationToken ct)
    {
        var allOrgs = await _orgs.Query()
            .Select(o => new { Id = o.Id.Value, ParentId = o.ParentId.HasValue ? o.ParentId.Value.Value : (Guid?)null })
            .ToListAsync(ct);

        var childrenMap = allOrgs
            .Where(o => o.ParentId.HasValue)
            .GroupBy(o => o.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(o => o.Id).ToList());

        var result = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(orgId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current != orgId) result.Add(current);
            if (childrenMap.TryGetValue(current, out var children))
                foreach (var child in children)
                    queue.Enqueue(child);
        }

        return result;
    }
}
