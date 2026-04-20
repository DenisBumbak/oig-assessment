using OIG.Application.Features.Users.GetById;
using OIG.Domain.Users;

namespace OIG.Application.Tests;

public class GetUserByIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserMissing()
    {
        var users = new FakeUserRepository();
        var roles = new FakeRoleRepository();
        var handler = new GetUserByIdHandler(users, roles);

        var result = await handler.Handle(new GetUserByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedResponse_WhenUserExists()
    {
        var users = new FakeUserRepository();
        var roles = new FakeRoleRepository();
        var user = User.Register("Member", "member@example.com");
        await users.AddAsync(user, CancellationToken.None);
        var handler = new GetUserByIdHandler(users, roles);

        var result = await handler.Handle(new GetUserByIdQuery(user.Id.Value), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id.Value, result.Id);
        Assert.Equal("Member", result.Name);
        Assert.Equal("member@example.com", result.Email);
        Assert.True(result.IsActive);
        Assert.Empty(result.Roles);
    }
}
