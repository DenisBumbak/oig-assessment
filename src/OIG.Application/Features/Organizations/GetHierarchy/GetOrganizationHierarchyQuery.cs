using MediatR;

namespace OIG.Application.Features.Organizations.GetHierarchy;

public sealed record GetOrganizationHierarchyQuery() : IRequest<GetOrganizationHierarchyResponse>;
