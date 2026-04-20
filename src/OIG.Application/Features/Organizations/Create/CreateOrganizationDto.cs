namespace OIG.Application.Features.Organizations.Create;

public sealed record CreateOrganizationRequest(string Name, Guid? ParentId);
public sealed record CreateOrganizationResponse(Guid Id, string Name, Guid? ParentId);
