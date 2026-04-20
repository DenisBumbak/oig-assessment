using OIG.Domain.Common;
using OIG.Domain.Organizations;
using OIG.Domain.Roles;
using OIG.Domain.Users;
using System.Linq;

namespace OIG.Application.Tests;

public class EmailTests
{
    [Fact]
    public void Create_ShouldNormalizeToLowercase()
    {
        var email = Email.Create("  USER@Example.COM  ");

        Assert.Equal("user@example.com", email.Value);
    }

    [Fact]
    public void Create_ShouldThrow_WhenInvalidFormat()
    {
        Assert.Throws<DomainException>(() => Email.Create("invalid"));
    }
}

public class OrganizationTests
{
    [Fact]
    public void CreateRoot_ShouldSetNoParent()
    {
        var org = Organization.CreateRoot("Root");

        Assert.Equal("Root", org.Name);
        Assert.Null(org.ParentId);
    }

    [Fact]
    public void CreateChild_ShouldSetParentId()
    {
        var parent = Organization.CreateRoot("Parent");

        var child = Organization.CreateChild("Child", parent);

        Assert.Equal(parent.Id, child.ParentId);
    }

    [Fact]
    public void Rename_ShouldThrow_WhenNameInvalid()
    {
        var org = Organization.CreateRoot("Root");

        Assert.Throws<DomainException>(() => org.Rename(" "));
    }
}

public class UserTests
{
    [Fact]
    public void Register_ShouldCreateActiveUser()
    {
        var user = User.Register("John Doe", "user1@example.com");

        Assert.True(user.IsActive);
        Assert.Equal("John Doe", user.Name);
        Assert.Equal("user1@example.com", user.Email.Value);
    }

    [Fact]
    public void ChangeEmail_ShouldNormalize()
    {
        var user = User.Register("John Doe", "user1@example.com");

        user.ChangeEmail("NEW@Example.com");

        Assert.Equal("new@example.com", user.Email.Value);
    }

    [Fact]
    public void ChangeName_ShouldUpdate()
    {
        var user = User.Register("John Doe", "user1@example.com");

        user.ChangeName("Jane Doe");

        Assert.Equal("Jane Doe", user.Name);
    }

    [Fact]
    public void ChangeName_ShouldThrow_WhenEmpty()
    {
        var user = User.Register("John Doe", "user@example.com");

        Assert.Throws<DomainException>(() => user.ChangeName("  "));
    }

    [Fact]
    public void Deactivate_ThenActivate_ShouldToggleState()
    {
        var user = User.Register("John Doe", "user@example.com");

        user.Deactivate();
        user.Activate();

        Assert.True(user.IsActive);
    }

    [Fact]
    public void AssignRole_ShouldAddRole()
    {
        var user = User.Register("John Doe", "user@example.com");
        var roleId = RoleId.New();

        user.AssignRole(roleId);

        Assert.Single(user.UserRoles);
        Assert.Equal(roleId, user.UserRoles.First().RoleId);
    }

    [Fact]
    public void AssignRole_ShouldNotDuplicate()
    {
        var user = User.Register("John Doe", "user@example.com");
        var roleId = RoleId.New();

        user.AssignRole(roleId);
        user.AssignRole(roleId);

        Assert.Single(user.UserRoles);
    }

    [Fact]
    public void RemoveRole_ShouldRemove()
    {
        var user = User.Register("John Doe", "user@example.com");
        var roleId = RoleId.New();

        user.AssignRole(roleId);
        user.RemoveRole(roleId);

        Assert.Empty(user.UserRoles);
    }
}

public class RoleTests
{
    [Fact]
    public void Grant_ShouldAvoidDuplicates()
    {
        var role = Role.Create("Admin");

        role.Grant(Permission.ViewUserList);
        role.Grant(Permission.ViewUserList);

        Assert.Single(role.Permissions);
    }

    [Fact]
    public void Revoke_ShouldRemovePermission()
    {
        var role = Role.Create("Admin");
        role.Grant(Permission.ViewUserList);

        role.Revoke(Permission.ViewUserList);

        Assert.Empty(role.Permissions);
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameEmpty()
    {
        Assert.Throws<DomainException>(() => Role.Create("  "));
    }
}
