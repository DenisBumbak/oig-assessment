using OIG.Application.Features.Organizations.Delete;
using OIG.Application.Features.Organizations.GetById;
using OIG.Application.Features.Organizations.Update;
using OIG.Domain.Organizations;

namespace OIG.Application.Tests;

public class GetOrganizationByIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotFound()
    {
        var orgs = new FakeOrganizationRepository();
        var handler = new GetOrganizationByIdHandler(orgs);

        var result = await handler.Handle(new GetOrganizationByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrg_WhenExists()
    {
        var orgs = new FakeOrganizationRepository();
        var org = Organization.CreateRoot("HQ");
        await orgs.AddAsync(org, CancellationToken.None);
        var handler = new GetOrganizationByIdHandler(orgs);

        var result = await handler.Handle(new GetOrganizationByIdQuery(org.Id.Value), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("HQ", result.Name);
        Assert.Null(result.ParentId);
    }
}

public class UpdateOrganizationHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRenamOrganization()
    {
        var orgs = new FakeOrganizationRepository();
        var org = Organization.CreateRoot("Old");
        await orgs.AddAsync(org, CancellationToken.None);
        var uow = new FakeUnitOfWork();
        var handler = new UpdateOrganizationHandler(orgs, uow);

        var result = await handler.Handle(
            new UpdateOrganizationCommand(org.Id.Value, "New", null),
            CancellationToken.None);

        Assert.Equal("New", result.Name);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNotFound()
    {
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var handler = new UpdateOrganizationHandler(orgs, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UpdateOrganizationCommand(Guid.NewGuid(), "X", null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSelfParent()
    {
        var orgs = new FakeOrganizationRepository();
        var org = Organization.CreateRoot("Root");
        await orgs.AddAsync(org, CancellationToken.None);
        var uow = new FakeUnitOfWork();
        var handler = new UpdateOrganizationHandler(orgs, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new UpdateOrganizationCommand(org.Id.Value, "Root", org.Id.Value),
                CancellationToken.None));
    }
}

public class DeleteOrganizationHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveOrganization()
    {
        var orgs = new FakeOrganizationRepository();
        var org = Organization.CreateRoot("ToDelete");
        await orgs.AddAsync(org, CancellationToken.None);
        var uow = new FakeUnitOfWork();
        var handler = new DeleteOrganizationHandler(orgs, uow);

        await handler.Handle(new DeleteOrganizationCommand(org.Id.Value), CancellationToken.None);

        Assert.Null(await orgs.FindByIdAsync(org.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNotFound()
    {
        var orgs = new FakeOrganizationRepository();
        var uow = new FakeUnitOfWork();
        var handler = new DeleteOrganizationHandler(orgs, uow);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new DeleteOrganizationCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
