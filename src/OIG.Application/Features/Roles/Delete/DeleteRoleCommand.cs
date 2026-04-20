using MediatR;

namespace OIG.Application.Features.Roles.Delete;

public sealed record DeleteRoleCommand(Guid Id) : IRequest;
