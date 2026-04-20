namespace OIG.BlazorApp.Services;

// Organizations
public record OrganizationDto(Guid Id, string Name, Guid? ParentId);
public record OrganizationNodeDto(Guid Id, string Name, Guid? ParentId, List<OrganizationNodeDto> Children);
public record OrganizationHierarchyResponse(List<OrganizationNodeDto> Roots);
public record CreateOrganizationRequest(string Name, Guid? ParentId);
public record UpdateOrganizationRequest(string Name, Guid? ParentId);

// Users
public record UserListItemDto(Guid Id, string Name, string Email, bool IsActive, Guid? OrganizationId);
public record UsersListResponse(List<UserListItemDto> Items);
public record UserRoleDto(Guid RoleId, string RoleName);
public record UserDetailDto(Guid Id, string Name, string Email, bool IsActive, Guid? OrganizationId, List<UserRoleDto> Roles);
public record CreateUserRequest(string Name, string Email, Guid? OrganizationId);
public record UpdateUserRequest(string Name, string Email, Guid? OrganizationId, List<Guid> RoleIds);
public record CreateUserResponse(Guid Id, string Name, string Email, bool IsActive, Guid? OrganizationId);

// Roles
public enum Permission
{
    None = 0, ViewUserList = 1, AddUser = 2, EditUser = 3,
    ViewRolesList = 4, AddRole = 5, EditRole = 6,
    ViewOrganizations = 7, EditOrganization = 8
}
public record RoleListItemDto(Guid Id, string Name, Guid? OrganizationId, List<Permission> Permissions);
public record RolesListResponse(List<RoleListItemDto> Items);
public record RoleDetailDto(Guid Id, string Name, Guid? OrganizationId, List<Permission> Permissions);
public record CreateRoleRequest(string Name, Guid? OrganizationId, List<Permission> Permissions);
public record UpdateRoleRequest(string Name, List<Permission> Permissions);
