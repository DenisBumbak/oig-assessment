namespace OIG.Application.Features.Users.GetById;

public sealed record UserRoleDto(Guid RoleId, string RoleName);
public sealed record GetUserByIdResponse(Guid Id, string Name, string Email, bool IsActive, Guid? OrganizationId, List<UserRoleDto> Roles);
