namespace OIG.Application.Features.Organizations.Update;

public sealed record UpdateOrganizationRequest(string Name, Guid? ParentId);
public sealed record UpdateOrganizationResponse(Guid Id, string Name, Guid? ParentId);
