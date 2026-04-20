namespace OIG.WebApi.Endpoints;

public static class EndpointGroupExtensions
{
    public static void MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").WithOpenApi();
        api.MapUsersEndpoints();
        api.MapOrganizationsEndpoints();
        api.MapRolesEndpoints();
    }
}
