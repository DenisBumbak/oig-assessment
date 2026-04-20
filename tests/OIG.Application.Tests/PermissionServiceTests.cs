using OIG.Application.Abstractions.Auth;
using OIG.Application.Abstractions.Persistence;
using OIG.Domain.Organizations;
using OIG.Domain.Roles;
using OIG.Domain.Users;
using OIG.Infrastructure.Auth;

namespace OIG.Application.Tests;

public class PermissionServiceTests
{
    [Fact]
    public async Task GetEffectivePermissions_ShouldReturnEmpty_WhenUserHasNoRoles()
    {
        var (users, roles, orgs) = CreateRepos();
        var user = User.Register("U", "u@e.com");
        await users.AddAsync(user, CancellationToken.None);
        var svc = new PermissionService(users, roles, orgs);

        var perms = await svc.GetEffectivePermissionsAsync(user.Id, CancellationToken.None);

        Assert.Empty(perms);
    }

    [Fact]
    public async Task GetEffectivePermissions_ShouldIncludePermissions_FromRoleInSameOrg()
    {
        var (users, roles, orgs) = CreateRepos();
        var org = Organization.CreateRoot("Org");
        await orgs.AddAsync(org, CancellationToken.None);

        var role = Role.Create("Editor", org.Id.Value);
        role.Grant(Permission.ViewUserList);
        role.Grant(Permission.AddUser);
        await roles.AddAsync(role, CancellationToken.None);

        var user = User.Register("U", "u@e.com", org.Id.Value);
        user.AssignRole(role.Id);
        await users.AddAsync(user, CancellationToken.None);

        var svc = new PermissionService(users, roles, orgs);

        var perms = await svc.GetEffectivePermissionsAsync(user.Id, CancellationToken.None);

        Assert.Contains(Permission.ViewUserList, perms);
        Assert.Contains(Permission.AddUser, perms);
    }

    [Fact]
    public async Task GetEffectivePermissions_ShouldIncludePermissions_FromRoleInParentOrg()
    {
        var (users, roles, orgs) = CreateRepos();
        var parent = Organization.CreateRoot("Parent");
        var child = Organization.CreateChild("Child", parent);
        await orgs.AddAsync(parent, CancellationToken.None);
        await orgs.AddAsync(child, CancellationToken.None);

        var role = Role.Create("ParentRole", parent.Id.Value);
        role.Grant(Permission.ViewRolesList);
        await roles.AddAsync(role, CancellationToken.None);

        var user = User.Register("U", "u@e.com", child.Id.Value);
        user.AssignRole(role.Id);
        await users.AddAsync(user, CancellationToken.None);

        var svc = new PermissionService(users, roles, orgs);

        var perms = await svc.GetEffectivePermissionsAsync(user.Id, CancellationToken.None);

        Assert.Contains(Permission.ViewRolesList, perms);
    }

    [Fact]
    public async Task GetEffectivePermissions_ShouldExcludePermissions_FromRoleInUnrelatedOrg()
    {
        var (users, roles, orgs) = CreateRepos();
        var orgA = Organization.CreateRoot("A");
        var orgB = Organization.CreateRoot("B");
        await orgs.AddAsync(orgA, CancellationToken.None);
        await orgs.AddAsync(orgB, CancellationToken.None);

        var role = Role.Create("OrgBRole", orgB.Id.Value);
        role.Grant(Permission.EditUser);
        await roles.AddAsync(role, CancellationToken.None);

        var user = User.Register("U", "u@e.com", orgA.Id.Value);
        user.AssignRole(role.Id);
        await users.AddAsync(user, CancellationToken.None);

        var svc = new PermissionService(users, roles, orgs);

        var perms = await svc.GetEffectivePermissionsAsync(user.Id, CancellationToken.None);

        Assert.DoesNotContain(Permission.EditUser, perms);
    }

    [Fact]
    public async Task HasPermission_ShouldReturnTrue_WhenUserHasPermission()
    {
        var (users, roles, orgs) = CreateRepos();
        var org = Organization.CreateRoot("Org");
        await orgs.AddAsync(org, CancellationToken.None);

        var role = Role.Create("R", org.Id.Value);
        role.Grant(Permission.ViewUserList);
        await roles.AddAsync(role, CancellationToken.None);

        var user = User.Register("U", "u@e.com", org.Id.Value);
        user.AssignRole(role.Id);
        await users.AddAsync(user, CancellationToken.None);

        var svc = new PermissionService(users, roles, orgs);

        Assert.True(await svc.HasPermissionAsync(user.Id, Permission.ViewUserList, CancellationToken.None));
        Assert.False(await svc.HasPermissionAsync(user.Id, Permission.EditUser, CancellationToken.None));
    }

    private static (FakeUserRepository, FakeRoleRepository, FakeOrganizationRepository) CreateRepos()
        => (new FakeUserRepository(), new FakeRoleRepository(), new FakeOrganizationRepository());
}
