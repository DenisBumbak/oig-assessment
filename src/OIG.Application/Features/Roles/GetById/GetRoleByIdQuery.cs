using MediatR;

namespace OIG.Application.Features.Roles.GetById;

public sealed record GetRoleByIdQuery(Guid Id) : IRequest<GetRoleByIdResponse?>;
