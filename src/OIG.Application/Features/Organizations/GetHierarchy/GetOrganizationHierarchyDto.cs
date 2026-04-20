namespace OIG.Application.Features.Organizations.GetHierarchy;

public sealed record OrganizationNodeDto(Guid Id, string Name, Guid? ParentId, List<OrganizationNodeDto> Children);
public sealed record GetOrganizationHierarchyResponse(List<OrganizationNodeDto> Roots);
