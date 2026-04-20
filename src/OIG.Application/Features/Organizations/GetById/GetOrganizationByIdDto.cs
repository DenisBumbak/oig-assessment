namespace OIG.Application.Features.Organizations.GetById;

public sealed record GetOrganizationByIdResponse(Guid Id, string Name, Guid? ParentId);
