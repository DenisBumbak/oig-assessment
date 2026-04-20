using MediatR;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Users;

namespace OIG.Application.Features.Users.Delete;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;

    public DeleteUserHandler(IUserRepository users, IUnitOfWork uow)
    {
        _users = users;
        _uow = uow;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(new UserId(request.Id), ct)
                   ?? throw new InvalidOperationException("User not found.");

        _users.Remove(user);
        await _uow.SaveChangesAsync(ct);
    }
}
