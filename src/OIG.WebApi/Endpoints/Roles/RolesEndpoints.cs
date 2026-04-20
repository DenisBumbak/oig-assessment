using MediatR;
using OIG.Application.Features.Roles.Create;
using OIG.Application.Features.Roles.Delete;
using OIG.Application.Features.Roles.GetById;
using OIG.Application.Features.Roles.GetList;
using OIG.Application.Features.Roles.Update;
using OIG.Domain.Roles;
using OIG.WebApi.Auth;

namespace OIG.WebApi.Endpoints.Roles;

public static class RolesEndpoints
{
    public static RouteGroupBuilder MapRolesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/roles").WithTags("Roles");

        group.MapPost("/", CreateRole)
            .WithName("CreateRole")
            .Produces<CreateRoleResponse>(StatusCodes.Status201Created)
            .RequirePermission(Permission.AddRole);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetRoleById")
            .Produces<GetRoleByIdResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(Permission.ViewRolesList);

        group.MapGet("/", GetList)
            .WithName("GetRolesList")
            .Produces<GetRolesListResponse>()
            .RequirePermission(Permission.ViewRolesList);

        group.MapPut("/{id:guid}", UpdateRole)
            .WithName("UpdateRole")
            .Produces<UpdateRoleResponse>()
            .RequirePermission(Permission.EditRole);

        group.MapDelete("/{id:guid}", DeleteRole)
            .WithName("DeleteRole")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(Permission.EditRole);

        return group;
    }

    private static async Task<IResult> CreateRole(CreateRoleRequest request, IMediator mediator, CancellationToken ct)
    {
        var created = await mediator.Send(
            new CreateRoleCommand(request.Name, request.OrganizationId, request.Permissions), ct);
        return Results.Created($"/api/roles/{created.Id}", created);
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var role = await mediator.Send(new GetRoleByIdQuery(id), ct);
        return role is null ? Results.NotFound() : Results.Ok(role);
    }

    private static async Task<IResult> GetList(Guid? organizationId, string? search, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetRolesListQuery(organizationId, search), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateRole(Guid id, UpdateRoleRequest request, IMediator mediator, CancellationToken ct)
    {
        var updated = await mediator.Send(
            new UpdateRoleCommand(id, request.Name, request.Permissions), ct);
        return Results.Ok(updated);
    }

    private static async Task<IResult> DeleteRole(Guid id, IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new DeleteRoleCommand(id), ct);
        return Results.NoContent();
    }
}
