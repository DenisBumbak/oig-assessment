using OIG.Application.Features.Users.GetList;
using OIG.Domain.Users;

namespace OIG.Application.Tests;

public class SearchTests
{
    [Fact]
    public async Task UserSearch_ShouldFilterByPartialName()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        await users.AddAsync(User.Register("Alice Smith", "alice@e.com"), CancellationToken.None);
        await users.AddAsync(User.Register("Bob Jones", "bob@e.com"), CancellationToken.None);
        await users.AddAsync(User.Register("Alice Johnson", "alicej@e.com"), CancellationToken.None);
        var handler = new GetUsersListHandler(users, orgs);

        var result = await handler.Handle(new GetUsersListQuery(null, "alice"), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, i => Assert.Contains("Alice", i.Name));
    }

    [Fact]
    public async Task UserSearch_ShouldBeCaseInsensitive()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        await users.AddAsync(User.Register("ALICE", "alice@e.com"), CancellationToken.None);
        var handler = new GetUsersListHandler(users, orgs);

        var result = await handler.Handle(new GetUsersListQuery(null, "alice"), CancellationToken.None);

        Assert.Single(result.Items);
    }

    [Fact]
    public async Task UserSearch_ShouldReturnEmpty_WhenNoMatch()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        await users.AddAsync(User.Register("Alice", "alice@e.com"), CancellationToken.None);
        var handler = new GetUsersListHandler(users, orgs);

        var result = await handler.Handle(new GetUsersListQuery(null, "zzz"), CancellationToken.None);

        Assert.Empty(result.Items);
    }
}
