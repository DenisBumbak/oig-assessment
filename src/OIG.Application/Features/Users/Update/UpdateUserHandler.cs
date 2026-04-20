using MediatR;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Organizations;
using OIG.Domain.Roles;
using OIG.Domain.Users;

namespace OIG.Application.Features.Users.Update;

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IUserRepository _users;
    private readonly IOrganizationRepository _orgs;
    private readonly IUnitOfWork _uow;

    public UpdateUserHandler(IUserRepository users, IOrganizationRepository orgs, IUnitOfWork uow)
    {
        _users = users;
        _orgs = orgs;
        _uow = uow;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await _users.FindByIdWithRolesAsync(new UserId(request.Id), ct)
                   ?? throw new InvalidOperationException("User not found.");

        if (request.OrganizationId.HasValue)
        {
            var org = await _orgs.FindByIdAsync(new OrganizationId(request.OrganizationId.Value), ct);
            if (org is null)
                throw new InvalidOperationException("Organization not found.");
        }

        var normalizedEmail = Email.Create(request.Email).Value;
        var existing = await _users.FindByEmailAsync(normalizedEmail, ct);
        if (existing is not null && existing.Id != user.Id)
            throw new InvalidOperationException("User with this email already exists.");

        user.ChangeName(request.Name);
        user.ChangeEmail(request.Email);
        user.AssignToOrganization(request.OrganizationId);

        var currentRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToHashSet();
        var desiredRoleIds = request.RoleIds.Select(id => new RoleId(id)).ToHashSet();

        foreach (var toRemove in currentRoleIds.Except(desiredRoleIds))
            user.RemoveRole(toRemove);

        foreach (var toAdd in desiredRoleIds.Except(currentRoleIds))
            user.AssignRole(toAdd);

        await _uow.SaveChangesAsync(ct);

        return new UpdateUserResponse(
            user.Id.Value,
            user.Name,
            user.Email.Value,
            user.IsActive,
            user.OrganizationId,
            user.UserRoles.Select(ur => ur.RoleId.Value).ToList());
    }
}
