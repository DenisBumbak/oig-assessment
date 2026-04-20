using MediatR;
using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.Create;

public sealed record CreateRoleCommand(string Name, Guid? OrganizationId, List<Permission> Permissions) : IRequest<CreateRoleResponse>;
