namespace OIG.Application.Features.Users.Create;

public sealed record CreateUserRequest(string Name, string Email, Guid? OrganizationId);
public sealed record CreateUserResponse(Guid Id, string Name, string Email, bool IsActive, Guid? OrganizationId);
