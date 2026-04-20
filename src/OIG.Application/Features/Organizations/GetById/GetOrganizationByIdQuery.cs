using MediatR;

namespace OIG.Application.Features.Organizations.GetById;

public sealed record GetOrganizationByIdQuery(Guid Id) : IRequest<GetOrganizationByIdResponse?>;
