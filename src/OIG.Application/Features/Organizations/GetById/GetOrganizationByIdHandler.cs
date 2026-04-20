using MediatR;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Organizations;

namespace OIG.Application.Features.Organizations.GetById;

public sealed class GetOrganizationByIdHandler : IRequestHandler<GetOrganizationByIdQuery, GetOrganizationByIdResponse?>
{
    private readonly IOrganizationRepository _orgs;

    public GetOrganizationByIdHandler(IOrganizationRepository orgs) => _orgs = orgs;

    public async Task<GetOrganizationByIdResponse?> Handle(GetOrganizationByIdQuery request, CancellationToken ct)
    {
        var org = await _orgs.FindByIdAsync(new OrganizationId(request.Id), ct);
        return org is null ? null : new GetOrganizationByIdResponse(org.Id.Value, org.Name, org.ParentId?.Value);
    }
}
