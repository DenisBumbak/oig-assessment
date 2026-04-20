using MediatR;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Organizations;

namespace OIG.Application.Features.Organizations.Create;

public sealed class CreateOrganizationHandler : IRequestHandler<CreateOrganizationCommand, CreateOrganizationResponse>
{
    private readonly IOrganizationRepository _orgs;
    private readonly IUnitOfWork _uow;

    public CreateOrganizationHandler(IOrganizationRepository orgs, IUnitOfWork uow)
    {
        _orgs = orgs;
        _uow = uow;
    }

    public async Task<CreateOrganizationResponse> Handle(CreateOrganizationCommand request, CancellationToken ct)
    {
        Organization org;

        if (request.ParentId is null)
        {
            org = Organization.CreateRoot(request.Name);
        }
        else
        {
            var parent = await _orgs.FindByIdAsync(new OrganizationId(request.ParentId.Value), ct)
                         ?? throw new InvalidOperationException("Parent organization not found.");
            org = Organization.CreateChild(request.Name, parent);
        }

        await _orgs.AddAsync(org, ct);
        await _uow.SaveChangesAsync(ct);

        return new CreateOrganizationResponse(org.Id.Value, org.Name, org.ParentId?.Value);
    }
}
