using MediatR;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.Update;

public sealed class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, UpdateRoleResponse>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _uow;

    public UpdateRoleHandler(IRoleRepository roles, IUnitOfWork uow)
    {
        _roles = roles;
        _uow = uow;
    }

    public async Task<UpdateRoleResponse> Handle(UpdateRoleCommand request, CancellationToken ct)
    {
        var role = await _roles.FindByIdAsync(new RoleId(request.Id), ct)
                   ?? throw new InvalidOperationException("Role not found.");

        role.Rename(request.Name);
        role.ReplacePermissions(request.Permissions);

        await _uow.SaveChangesAsync(ct);

        return new UpdateRoleResponse(
            role.Id.Value,
            role.Name,
            role.OrganizationId,
            role.Permissions.Select(p => p.Permission).ToList());
    }
}
