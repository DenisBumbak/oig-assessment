using MediatR;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.GetById;

public sealed class GetRoleByIdHandler : IRequestHandler<GetRoleByIdQuery, GetRoleByIdResponse?>
{
    private readonly IRoleRepository _roles;

    public GetRoleByIdHandler(IRoleRepository roles) => _roles = roles;

    public async Task<GetRoleByIdResponse?> Handle(GetRoleByIdQuery request, CancellationToken ct)
    {
        var role = await _roles.FindByIdAsync(new RoleId(request.Id), ct);
        if (role is null) return null;

        return new GetRoleByIdResponse(
            role.Id.Value,
            role.Name,
            role.OrganizationId,
            role.Permissions.Select(p => p.Permission).ToList());
    }
}
