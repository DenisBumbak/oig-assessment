using MediatR;
using Microsoft.EntityFrameworkCore;
using OIG.Application.Abstractions.Persistence;

namespace OIG.Application.Features.Users.GetList;

public sealed class GetUsersListHandler : IRequestHandler<GetUsersListQuery, GetUsersListResponse>
{
    private readonly IUserRepository _users;
    private readonly IOrganizationRepository _orgs;

    public GetUsersListHandler(IUserRepository users, IOrganizationRepository orgs)
    {
        _users = users;
        _orgs = orgs;
    }

    public async Task<GetUsersListResponse> Handle(GetUsersListQuery request, CancellationToken ct)
    {
        var query = _users.Query();

        if (request.OrganizationId.HasValue)
        {
            var orgIds = await CollectOrgIdsWithDescendantsAsync(request.OrganizationId.Value, ct);
            query = query.Where(u => u.OrganizationId.HasValue && orgIds.Contains(u.OrganizationId.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(search));
        }

        var users = await query.ToListAsync(ct);

        var items = users.Select(u => new UserListItemDto(
            u.Id.Value, u.Name, u.Email.Value, u.IsActive, u.OrganizationId
        )).ToList();

        return new GetUsersListResponse(items);
    }

    private async Task<HashSet<Guid>> CollectOrgIdsWithDescendantsAsync(Guid rootOrgId, CancellationToken ct)
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
        queue.Enqueue(rootOrgId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            if (childrenMap.TryGetValue(current, out var children))
                foreach (var child in children)
                    queue.Enqueue(child);
        }

        return result;
    }
}
