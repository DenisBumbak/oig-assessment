using MediatR;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Organizations;

namespace OIG.Application.Features.Organizations.Delete;

public sealed class DeleteOrganizationHandler : IRequestHandler<DeleteOrganizationCommand>
{
    private readonly IOrganizationRepository _orgs;
    private readonly IUnitOfWork _uow;

    public DeleteOrganizationHandler(IOrganizationRepository orgs, IUnitOfWork uow)
    {
        _orgs = orgs;
        _uow = uow;
    }

    public async Task Handle(DeleteOrganizationCommand request, CancellationToken ct)
    {
        var org = await _orgs.FindByIdAsync(new OrganizationId(request.Id), ct)
                  ?? throw new InvalidOperationException("Organization not found.");

        _orgs.Remove(org);
        await _uow.SaveChangesAsync(ct);
    }
}
