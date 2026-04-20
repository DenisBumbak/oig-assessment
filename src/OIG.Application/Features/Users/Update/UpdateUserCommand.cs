using MediatR;

namespace OIG.Application.Features.Users.Update;

public sealed record UpdateUserCommand(
    Guid Id,
    string Name,
    string Email,
    Guid? OrganizationId,
    List<Guid> RoleIds) : IRequest<UpdateUserResponse>;
