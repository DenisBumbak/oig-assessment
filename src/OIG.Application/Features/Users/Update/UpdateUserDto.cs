namespace OIG.Application.Features.Users.Update;

public sealed record UpdateUserRequest(string Name, string Email, Guid? OrganizationId, List<Guid> RoleIds);
public sealed record UpdateUserResponse(Guid Id, string Name, string Email, bool IsActive, Guid? OrganizationId, List<Guid> RoleIds);
