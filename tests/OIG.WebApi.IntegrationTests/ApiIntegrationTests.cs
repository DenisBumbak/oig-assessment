using System.Net;
using System.Net.Http.Json;

namespace OIG.WebApi.IntegrationTests;

public class ApiIntegrationTests
{
    [Fact]
    public async Task CreateUser_ThenGetById_ShouldReturnCreatedUser()
    {
        using var factory = new TestWebAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        var email = $"owner-{Guid.NewGuid():N}@example.com";

        var orgResponse = await client.PostAsJsonAsync("/api/organizations/", new { name = "HQ", parentId = (Guid?)null });
        Assert.Equal(HttpStatusCode.Created, orgResponse.StatusCode);
        var org = await orgResponse.Content.ReadFromJsonAsync<CreateOrganizationResponse>();
        Assert.NotNull(org);

        var createUserResponse = await client.PostAsJsonAsync("/api/users/", new { name = "Owner", email, organizationId = org.Id });
        Assert.Equal(HttpStatusCode.Created, createUserResponse.StatusCode);
        var created = await createUserResponse.Content.ReadFromJsonAsync<CreateUserResponse>();
        Assert.NotNull(created);

        var getUserResponse = await client.GetAsync($"/api/users/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getUserResponse.StatusCode);
        var fetched = await getUserResponse.Content.ReadFromJsonAsync<GetUserByIdResponse>();
        Assert.NotNull(fetched);
        Assert.Equal("Owner", fetched.Name);
        Assert.Equal(email, fetched.Email);
        Assert.Equal(org.Id, fetched.OrganizationId);
    }

    [Fact]
    public async Task CreateUser_WithUnknownOrganization_ShouldReturnBadRequest()
    {
        using var factory = new TestWebAppFactory();
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/users/", new
        {
            name = "Test User",
            email = "user@example.com",
            organizationId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ShouldReturnConflict()
    {
        using var factory = new TestWebAppFactory();
        using var client = factory.CreateAuthenticatedClient();

        var first = await client.PostAsJsonAsync("/api/users/", new { name = "User One", email = "dup@example.com", organizationId = (Guid?)null });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/users/", new { name = "User Two", email = "dup@example.com", organizationId = (Guid?)null });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task GetHierarchy_ShouldReturnNestedOrganizations()
    {
        using var factory = new TestWebAppFactory();
        using var client = factory.CreateAuthenticatedClient();

        var parentResponse = await client.PostAsJsonAsync("/api/organizations/", new { name = "Parent", parentId = (Guid?)null });
        Assert.Equal(HttpStatusCode.Created, parentResponse.StatusCode);
        var parent = await parentResponse.Content.ReadFromJsonAsync<CreateOrganizationResponse>();
        Assert.NotNull(parent);

        var childResponse = await client.PostAsJsonAsync("/api/organizations/", new { name = "Child", parentId = parent.Id });
        Assert.Equal(HttpStatusCode.Created, childResponse.StatusCode);

        var hierarchyResponse = await client.GetAsync("/api/organizations/hierarchy");
        Assert.Equal(HttpStatusCode.OK, hierarchyResponse.StatusCode);
        var hierarchy = await hierarchyResponse.Content.ReadFromJsonAsync<GetOrganizationHierarchyResponse>();
        Assert.NotNull(hierarchy);

        Assert.True(hierarchy.Roots.Count >= 1);
        var parentNode = hierarchy.Roots.First(r => r.Name == "Parent");
        var child = Assert.Single(parentNode.Children);
        Assert.Equal("Child", child.Name);
    }

    [Fact]
    public async Task Request_WithoutUserIdHeader_ShouldReturn401()
    {
        using var factory = new TestWebAppFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_WithUserWithoutPermission_ShouldReturn403()
    {
        using var factory = new TestWebAppFactory();
        using var client = factory.CreateAuthenticatedClient();

        var userResponse = await client.PostAsJsonAsync("/api/users/", new
        {
            name = "No Perms",
            email = "noperms@example.com",
            organizationId = (Guid?)null
        });
        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);
        var user = await userResponse.Content.ReadFromJsonAsync<CreateUserResponse>();
        Assert.NotNull(user);

        using var noPermsClient = factory.CreateClient();
        noPermsClient.DefaultRequestHeaders.Add("X-User-Id", user.Id.ToString());

        var response = await noPermsClient.GetAsync("/api/users/");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record CreateOrganizationResponse(Guid Id, string Name, Guid? ParentId);
    private sealed record CreateUserResponse(Guid Id, string Name, string Email, bool IsActive, Guid? OrganizationId);
    private sealed record GetUserByIdResponse(Guid Id, string Name, string Email, bool IsActive, Guid? OrganizationId);
    private sealed record GetOrganizationHierarchyResponse(List<OrganizationNodeDto> Roots);
    private sealed record OrganizationNodeDto(Guid Id, string Name, Guid? ParentId, List<OrganizationNodeDto> Children);
}
