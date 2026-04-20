using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.GetById;

public sealed record GetRoleByIdResponse(Guid Id, string Name, Guid? OrganizationId, List<Permission> Permissions);
