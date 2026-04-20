using OIG.Application.Features.Organizations.Create;
using OIG.Application.Features.Organizations.GetHierarchy;
using OIG.Domain.Organizations;

namespace OIG.Application.Tests;

public class CreateOrganizationValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenNameEmpty()
    {
        var validator = new CreateOrganizationValidator();

        var result = await validator.ValidateAsync(new CreateOrganizationCommand("", null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenNameTooLong()
    {
        var validator = new CreateOrganizationValidator();

        var result = await validator.ValidateAsync(new CreateOrganizationCommand(new string('x', 201), null));

        Assert.False(result.IsValid);
    }
}

public class CreateOrganizationHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateRoot_WhenParentIsNull()
    {
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var handler = new CreateOrganizationHandler(orgs, uow);

        var result = await handler.Handle(new CreateOrganizationCommand("Root", null), CancellationToken.None);

        Assert.Equal("Root", result.Name);
        Assert.Null(result.ParentId);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_ShouldCreateChild_WhenParentExists()
    {
        var orgs = new FakeOrganizationRepository();
        var parent = Organization.CreateRoot("Parent");
        await orgs.AddAsync(parent, CancellationToken.None);

        var uow = new FakeUnitOfWork();
        var handler = new CreateOrganizationHandler(orgs, uow);

        var result = await handler.Handle(new CreateOrganizationCommand("Child", parent.Id.Value), CancellationToken.None);

        Assert.Equal("Child", result.Name);
        Assert.Equal(parent.Id.Value, result.ParentId);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenParentMissing()
    {
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var handler = new CreateOrganizationHandler(orgs, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateOrganizationCommand("Child", Guid.NewGuid()), CancellationToken.None));
    }
}

public class GetOrganizationHierarchyHandlerTests
{
    [Fact]
    public async Task Handle_ShouldBuildNestedTree()
    {
        var orgs = new FakeOrganizationRepository();
        var root = Organization.CreateRoot("Root");
        var child = Organization.CreateChild("Child", root);
        var grandChild = Organization.CreateChild("Grand", child);

        await orgs.AddAsync(root, CancellationToken.None);
        await orgs.AddAsync(child, CancellationToken.None);
        await orgs.AddAsync(grandChild, CancellationToken.None);

        var handler = new GetOrganizationHierarchyHandler(orgs);

        var result = await handler.Handle(new GetOrganizationHierarchyQuery(), CancellationToken.None);

        var rootNode = Assert.Single(result.Roots);
        Assert.Equal(root.Id.Value, rootNode.Id);

        var childNode = Assert.Single(rootNode.Children);
        Assert.Equal(child.Id.Value, childNode.Id);

        var grandChildNode = Assert.Single(childNode.Children);
        Assert.Equal(grandChild.Id.Value, grandChildNode.Id);
    }
}
