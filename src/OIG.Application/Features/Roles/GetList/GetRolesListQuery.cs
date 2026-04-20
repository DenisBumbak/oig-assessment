using MediatR;

namespace OIG.Application.Features.Roles.GetList;

public sealed record GetRolesListQuery(Guid? OrganizationId, string? Search = null) : IRequest<GetRolesListResponse>;
