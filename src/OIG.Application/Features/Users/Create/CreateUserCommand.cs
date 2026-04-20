using MediatR;

namespace OIG.Application.Features.Users.Create;

public sealed record CreateUserCommand(string Name, string Email, Guid? OrganizationId) : IRequest<CreateUserResponse>;
