using MediatR;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.Delete;

public sealed class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _uow;

    public DeleteRoleHandler(IRoleRepository roles, IUnitOfWork uow)
    {
        _roles = roles;
        _uow = uow;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var role = await _roles.FindByIdAsync(new RoleId(request.Id), ct)
                   ?? throw new InvalidOperationException("Role not found.");

        _roles.Remove(role);
        await _uow.SaveChangesAsync(ct);
    }
}
