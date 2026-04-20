using MediatR;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Organizations;
using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.Create;

public sealed class CreateRoleHandler : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    private readonly IRoleRepository _roles;
    private readonly IOrganizationRepository _orgs;
    private readonly IUnitOfWork _uow;

    public CreateRoleHandler(IRoleRepository roles, IOrganizationRepository orgs, IUnitOfWork uow)
    {
        _roles = roles;
        _orgs = orgs;
        _uow = uow;
    }

    public async Task<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        if (request.OrganizationId.HasValue)
        {
            var org = await _orgs.FindByIdAsync(new OrganizationId(request.OrganizationId.Value), ct);
            if (org is null)
                throw new InvalidOperationException("Organization not found.");
        }

        var role = Role.Create(request.Name, request.OrganizationId);

        foreach (var permission in request.Permissions)
            role.Grant(permission);

        await _roles.AddAsync(role, ct);
        await _uow.SaveChangesAsync(ct);

        return new CreateRoleResponse(
            role.Id.Value,
            role.Name,
            role.OrganizationId,
            role.Permissions.Select(p => p.Permission).ToList());
    }
}
