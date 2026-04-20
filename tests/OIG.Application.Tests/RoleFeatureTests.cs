using OIG.Application.Features.Roles.Create;
using OIG.Application.Features.Roles.Delete;
using OIG.Application.Features.Roles.GetById;
using OIG.Application.Features.Roles.GetList;
using OIG.Application.Features.Roles.Update;
using OIG.Domain.Organizations;
using OIG.Domain.Roles;

namespace OIG.Application.Tests;

public class CreateRoleHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateRoleWithPermissions()
    {
        var roles = new FakeRoleRepository();
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var handler = new CreateRoleHandler(roles, orgs, uow);

        var response = await handler.Handle(
            new CreateRoleCommand("Admin", null, [Permission.ViewUserList, Permission.AddUser]),
            CancellationToken.None);

        Assert.Equal("Admin", response.Name);
        Assert.Null(response.OrganizationId);
        Assert.Contains(Permission.ViewUserList, response.Permissions);
        Assert.Contains(Permission.AddUser, response.Permissions);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenOrganizationNotFound()
    {
        var roles = new FakeRoleRepository();
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var handler = new CreateRoleHandler(roles, orgs, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new CreateRoleCommand("Admin", Guid.NewGuid(), []),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCreateRoleForOrganization()
    {
        var roles = new FakeRoleRepository();
        var orgs = new FakeOrganizationRepository();
        var org = Organization.CreateRoot("HQ");
        await orgs.AddAsync(org, CancellationToken.None);
        var uow = new FakeUnitOfWork();
        var handler = new CreateRoleHandler(roles, orgs, uow);

        var response = await handler.Handle(
            new CreateRoleCommand("Manager", org.Id.Value, [Permission.ViewUserList]),
            CancellationToken.None);

        Assert.Equal(org.Id.Value, response.OrganizationId);
    }
}

public class CreateRoleValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenNameEmpty()
    {
        var orgs = new FakeOrganizationRepository();
        var validator = new CreateRoleValidator(orgs);

        var result = await validator.ValidateAsync(new CreateRoleCommand("", null, []));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenNameTooLong()
    {
        var orgs = new FakeOrganizationRepository();
        var validator = new CreateRoleValidator(orgs);

        var result = await validator.ValidateAsync(new CreateRoleCommand(new string('x', 101), null, []));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenOrgNotFound()
    {
        var orgs = new FakeOrganizationRepository();
        var validator = new CreateRoleValidator(orgs);

        var result = await validator.ValidateAsync(new CreateRoleCommand("Admin", Guid.NewGuid(), []));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Organization not found.");
    }
}

public class GetRoleByIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNull_WhenRoleMissing()
    {
        var roles = new FakeRoleRepository();
        var handler = new GetRoleByIdHandler(roles);

        var result = await handler.Handle(new GetRoleByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnRole_WhenExists()
    {
        var roles = new FakeRoleRepository();
        var role = Role.Create("Editor");
        role.Grant(Permission.ViewUserList);
        await roles.AddAsync(role, CancellationToken.None);
        var handler = new GetRoleByIdHandler(roles);

        var result = await handler.Handle(new GetRoleByIdQuery(role.Id.Value), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Editor", result.Name);
        Assert.Contains(Permission.ViewUserList, result.Permissions);
    }
}

public class UpdateRoleHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateNameAndPermissions()
    {
        var roles = new FakeRoleRepository();
        var role = Role.Create("Old Name");
        role.Grant(Permission.ViewUserList);
        await roles.AddAsync(role, CancellationToken.None);
        var uow = new FakeUnitOfWork();
        var handler = new UpdateRoleHandler(roles, uow);

        var result = await handler.Handle(
            new UpdateRoleCommand(role.Id.Value, "New Name", [Permission.AddUser, Permission.EditUser]),
            CancellationToken.None);

        Assert.Equal("New Name", result.Name);
        Assert.DoesNotContain(Permission.ViewUserList, result.Permissions);
        Assert.Contains(Permission.AddUser, result.Permissions);
        Assert.Contains(Permission.EditUser, result.Permissions);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenRoleNotFound()
    {
        var roles = new FakeRoleRepository();
        var uow = new FakeUnitOfWork();
        var handler = new UpdateRoleHandler(roles, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new UpdateRoleCommand(Guid.NewGuid(), "Name", []),
                CancellationToken.None));
    }
}

public class DeleteRoleHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveRole()
    {
        var roles = new FakeRoleRepository();
        var role = Role.Create("ToDelete");
        await roles.AddAsync(role, CancellationToken.None);
        var uow = new FakeUnitOfWork();
        var handler = new DeleteRoleHandler(roles, uow);

        await handler.Handle(new DeleteRoleCommand(role.Id.Value), CancellationToken.None);

        Assert.Null(await roles.FindByIdAsync(role.Id, CancellationToken.None));
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenRoleNotFound()
    {
        var roles = new FakeRoleRepository();
        var uow = new FakeUnitOfWork();
        var handler = new DeleteRoleHandler(roles, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteRoleCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
