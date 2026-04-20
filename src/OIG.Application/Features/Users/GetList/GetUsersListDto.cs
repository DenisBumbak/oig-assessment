namespace OIG.Application.Features.Users.GetList;

public sealed record UserListItemDto(Guid Id, string Name, string Email, bool IsActive, Guid? OrganizationId);
public sealed record GetUsersListResponse(List<UserListItemDto> Items);
