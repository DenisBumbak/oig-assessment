using MediatR;

namespace OIG.Application.Features.Users.GetById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<GetUserByIdResponse?>;
