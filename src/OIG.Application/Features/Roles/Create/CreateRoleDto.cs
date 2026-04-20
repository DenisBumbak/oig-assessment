using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.Create;

public sealed record CreateRoleRequest(string Name, Guid? OrganizationId, List<Permission> Permissions);
public sealed record CreateRoleResponse(Guid Id, string Name, Guid? OrganizationId, List<Permission> Permissions);
