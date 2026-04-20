using OIG.Application.Features.Users.Delete;
using OIG.Application.Features.Users.GetList;
using OIG.Application.Features.Users.Update;
using OIG.Domain.Organizations;
using OIG.Domain.Roles;
using OIG.Domain.Users;

namespace OIG.Application.Tests;

public class UpdateUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateNameEmailAndRoles()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var user = User.Register("Old Name", "old@example.com");
        await users.AddAsync(user, CancellationToken.None);
        var handler = new UpdateUserHandler(users, orgs, uow);

        var roleId = Guid.NewGuid();
        var result = await handler.Handle(
            new UpdateUserCommand(user.Id.Value, "New Name", "new@example.com", null, [roleId]),
            CancellationToken.None);

        Assert.Equal("New Name", result.Name);
        Assert.Equal("new@example.com", result.Email);
        Assert.Contains(roleId, result.RoleIds);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFound()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var handler = new UpdateUserHandler(users, orgs, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new UpdateUserCommand(Guid.NewGuid(), "Name", "e@e.com", null, []),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenDuplicateEmail()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var user1 = User.Register("User1", "user1@example.com");
        var user2 = User.Register("User2", "user2@example.com");
        await users.AddAsync(user1, CancellationToken.None);
        await users.AddAsync(user2, CancellationToken.None);
        var handler = new UpdateUserHandler(users, orgs, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new UpdateUserCommand(user2.Id.Value, "User2", "user1@example.com", null, []),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldAllowKeepingSameEmail()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var user = User.Register("User", "same@example.com");
        await users.AddAsync(user, CancellationToken.None);
        var handler = new UpdateUserHandler(users, orgs, uow);

        var result = await handler.Handle(
            new UpdateUserCommand(user.Id.Value, "Updated", "same@example.com", null, []),
            CancellationToken.None);

        Assert.Equal("Updated", result.Name);
        Assert.Equal("same@example.com", result.Email);
    }

    [Fact]
    public async Task Handle_ShouldReplaceRoles()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var user = User.Register("User", "u@e.com");
        var oldRole = Guid.NewGuid();
        user.AssignRole(new RoleId(oldRole));
        await users.AddAsync(user, CancellationToken.None);
        var handler = new UpdateUserHandler(users, orgs, uow);

        var newRole = Guid.NewGuid();
        var result = await handler.Handle(
            new UpdateUserCommand(user.Id.Value, "User", "u@e.com", null, [newRole]),
            CancellationToken.None);

        Assert.DoesNotContain(oldRole, result.RoleIds);
        Assert.Contains(newRole, result.RoleIds);
    }
}

public class DeleteUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveUser()
    {
        var users = new FakeUserRepository();
        var uow = new FakeUnitOfWork();
        var user = User.Register("ToDelete", "del@example.com");
        await users.AddAsync(user, CancellationToken.None);
        var handler = new DeleteUserHandler(users, uow);

        await handler.Handle(new DeleteUserCommand(user.Id.Value), CancellationToken.None);

        Assert.Null(await users.FindByIdAsync(user.Id, CancellationToken.None));
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUserNotFound()
    {
        var users = new FakeUserRepository();
        var uow = new FakeUnitOfWork();
        var handler = new DeleteUserHandler(users, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteUserCommand(Guid.NewGuid()), CancellationToken.None));
    }
}

public class GetUsersListHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAllUsers_WhenNoOrgFilter()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();
        await users.AddAsync(User.Register("A", "a@e.com"), CancellationToken.None);
        await users.AddAsync(User.Register("B", "b@e.com"), CancellationToken.None);
        var handler = new GetUsersListHandler(users, orgs);

        var result = await handler.Handle(new GetUsersListQuery(null), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Handle_ShouldReturnUsersFromOrgAndDescendants()
    {
        var users = new FakeUserRepository();
        var orgs = new FakeOrganizationRepository();

        var root = Organization.CreateRoot("Root");
        var child = Organization.CreateChild("Child", root);
        var grandChild = Organization.CreateChild("GrandChild", child);
        var unrelated = Organization.CreateRoot("Other");

        await orgs.AddAsync(root, CancellationToken.None);
        await orgs.AddAsync(child, CancellationToken.None);
        await orgs.AddAsync(grandChild, CancellationToken.None);
        await orgs.AddAsync(unrelated, CancellationToken.None);

        var u1 = User.Register("Root User", "root@e.com", root.Id.Value);
        var u2 = User.Register("Child User", "child@e.com", child.Id.Value);
        var u3 = User.Register("Grand User", "grand@e.com", grandChild.Id.Value);
        var u4 = User.Register("Other User", "other@e.com", unrelated.Id.Value);

        await users.AddAsync(u1, CancellationToken.None);
        await users.AddAsync(u2, CancellationToken.None);
        await users.AddAsync(u3, CancellationToken.None);
        await users.AddAsync(u4, CancellationToken.None);

        var handler = new GetUsersListHandler(users, orgs);

        var result = await handler.Handle(new GetUsersListQuery(root.Id.Value), CancellationToken.None);

        Assert.Equal(3, result.Items.Count);
        Assert.Contains(result.Items, i => i.Name == "Root User");
        Assert.Contains(result.Items, i => i.Name == "Child User");
        Assert.Contains(result.Items, i => i.Name == "Grand User");
        Assert.DoesNotContain(result.Items, i => i.Name == "Other User");
    }
}
