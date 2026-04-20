using FluentValidation;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Organizations;

namespace OIG.Application.Features.Roles.Create;

public sealed class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator(IOrganizationRepository orgs)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

        RuleFor(x => x.OrganizationId)
            .MustAsync(async (orgId, ct) =>
            {
                if (!orgId.HasValue) return true;
                var org = await orgs.FindByIdAsync(new OrganizationId(orgId.Value), ct);
                return org is not null;
            })
            .WithMessage("Organization not found.");
    }
}
