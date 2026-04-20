using MediatR;

namespace OIG.Application.Features.Users.GetList;

public sealed record GetUsersListQuery(Guid? OrganizationId, string? Search = null) : IRequest<GetUsersListResponse>;
