using MediatR;
using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.Update;

public sealed record UpdateRoleCommand(Guid Id, string Name, List<Permission> Permissions) : IRequest<UpdateRoleResponse>;
