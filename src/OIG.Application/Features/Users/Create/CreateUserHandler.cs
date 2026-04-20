using MediatR;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Users;
using OIG.Domain.Organizations;

namespace OIG.Application.Features.Users.Create;

public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserRepository _users;
    private readonly IOrganizationRepository _orgs;
    private readonly IUnitOfWork _uow;

    public CreateUserHandler(IUserRepository users, IOrganizationRepository orgs, IUnitOfWork uow)
    {
        _users = users;
        _orgs = orgs;
        _uow = uow;
    }

    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken ct)
    {
        if (request.OrganizationId.HasValue)
        {
            var org = await _orgs.FindByIdAsync(new OrganizationId(request.OrganizationId.Value), ct);
            if (org is null)
                throw new InvalidOperationException("Organization not found.");
        }

        var normalized = Email.Create(request.Email).Value;

        var existing = await _users.FindByEmailAsync(normalized, ct);
        if (existing is not null)
            throw new InvalidOperationException("User with this email already exists.");

        var user = User.Register(request.Name, request.Email, request.OrganizationId);

        await _users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return new CreateUserResponse(user.Id.Value, user.Name, user.Email.Value, user.IsActive, user.OrganizationId);
    }
}
