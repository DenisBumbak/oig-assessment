using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.Update;

public sealed record UpdateRoleRequest(string Name, List<Permission> Permissions);
public sealed record UpdateRoleResponse(Guid Id, string Name, Guid? OrganizationId, List<Permission> Permissions);
