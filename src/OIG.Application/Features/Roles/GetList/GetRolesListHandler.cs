using MediatR;
using Microsoft.EntityFrameworkCore;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.GetList;

public sealed class GetRolesListHandler : IRequestHandler<GetRolesListQuery, GetRolesListResponse>
{
    private readonly IRoleRepository _roles;

    public GetRolesListHandler(IRoleRepository roles) => _roles = roles;

    public async Task<GetRolesListResponse> Handle(GetRolesListQuery request, CancellationToken ct)
    {
        IQueryable<Role> query = _roles.Query().Include(r => r.Permissions);

        if (request.OrganizationId.HasValue)
            query = query.Where(r => r.OrganizationId == request.OrganizationId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(search));
        }

        var roles = await query.ToListAsync(ct);

        var items = roles.Select(r => new RoleListItemDto(
            r.Id.Value,
            r.Name,
            r.OrganizationId,
            r.Permissions.Select(p => p.Permission).ToList()
        )).ToList();

        return new GetRolesListResponse(items);
    }
}
