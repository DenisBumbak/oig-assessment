using MediatR;
using Microsoft.EntityFrameworkCore;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Users;

namespace OIG.Application.Features.Users.GetById;

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdResponse?>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;

    public GetUserByIdHandler(IUserRepository users, IRoleRepository roles)
    {
        _users = users;
        _roles = roles;
    }

    public async Task<GetUserByIdResponse?> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _users.FindByIdWithRolesAsync(new UserId(request.Id), ct);
        if (user is null) return null;

        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var roles = roleIds.Count > 0
            ? await _roles.Query().Where(r => roleIds.Contains(r.Id)).ToListAsync(ct)
            : [];

        var roleDtos = roles.Select(r => new UserRoleDto(r.Id.Value, r.Name)).ToList();

        return new GetUserByIdResponse(
            user.Id.Value, user.Name, user.Email.Value,
            user.IsActive, user.OrganizationId, roleDtos);
    }
}
