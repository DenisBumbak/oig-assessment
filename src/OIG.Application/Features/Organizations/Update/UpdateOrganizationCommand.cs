using MediatR;

namespace OIG.Application.Features.Organizations.Update;

public sealed record UpdateOrganizationCommand(Guid Id, string Name, Guid? ParentId) : IRequest<UpdateOrganizationResponse>;
