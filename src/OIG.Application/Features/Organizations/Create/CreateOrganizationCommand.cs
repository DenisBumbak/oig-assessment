using MediatR;

namespace OIG.Application.Features.Organizations.Create;

public sealed record CreateOrganizationCommand(string Name, Guid? ParentId) : IRequest<CreateOrganizationResponse>;
