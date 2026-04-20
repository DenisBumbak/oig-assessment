using MediatR;

namespace OIG.Application.Features.Users.Delete;

public sealed record DeleteUserCommand(Guid Id) : IRequest;
