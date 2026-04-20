using MediatR;

namespace OIG.Application.Features.Organizations.Delete;

public sealed record DeleteOrganizationCommand(Guid Id) : IRequest;
