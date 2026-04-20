using OIG.Domain.Roles;

namespace OIG.Application.Features.Roles.GetList;

public sealed record RoleListItemDto(Guid Id, string Name, Guid? OrganizationId, List<Permission> Permissions);
public sealed record GetRolesListResponse(List<RoleListItemDto> Items);
