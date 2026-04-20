using FluentValidation;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Organizations;

namespace OIG.Application.Features.Users.Create;

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator(IOrganizationRepository orgs)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(320);

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
