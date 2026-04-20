using OIG.Application.Abstractions.Auth;
using OIG.Domain.Roles;

namespace OIG.WebApi.Auth;

public sealed class RequirePermissionFilter : IEndpointFilter
{
    private readonly Permission _permission;

    public RequirePermissionFilter(Permission permission) => _permission = permission;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserAccessor>();
        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

        if (currentUser.UserId is null)
            return Results.Problem(
                title: "Unauthorized",
                detail: "X-User-Id header is required.",
                statusCode: StatusCodes.Status401Unauthorized);

        var hasPermission = await permissionService.HasPermissionAsync(currentUser.UserId.Value, _permission, context.HttpContext.RequestAborted);

        if (!hasPermission)
            return Results.Problem(
                title: "Forbidden",
                detail: $"Missing permission: {_permission}.",
                statusCode: StatusCodes.Status403Forbidden);

        return await next(context);
    }
}

public static class EndpointFilterExtensions
{
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, Permission permission)
        => builder.AddEndpointFilter(new RequirePermissionFilter(permission));
}
