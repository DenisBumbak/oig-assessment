using MediatR;
using OIG.Application.Features.Users.Create;
using OIG.Application.Features.Users.Delete;
using OIG.Application.Features.Users.GetById;
using OIG.Application.Features.Users.GetList;
using OIG.Application.Features.Users.Update;
using OIG.Domain.Roles;
using OIG.WebApi.Auth;

namespace OIG.WebApi.Endpoints.Users;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/users").WithTags("Users");

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .RequirePermission(Permission.AddUser);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetUserById")
            .Produces<GetUserByIdResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(Permission.ViewUserList);

        group.MapGet("/", GetList)
            .WithName("GetUsersList")
            .Produces<GetUsersListResponse>()
            .RequirePermission(Permission.ViewUserList);

        group.MapPut("/{id:guid}", UpdateUser)
            .WithName("UpdateUser")
            .Produces<UpdateUserResponse>()
            .RequirePermission(Permission.EditUser);

        group.MapDelete("/{id:guid}", DeleteUser)
            .WithName("DeleteUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(Permission.EditUser);

        return group;
    }

    private static async Task<IResult> CreateUser(CreateUserRequest request, IMediator mediator, CancellationToken ct)
    {
        var created = await mediator.Send(new CreateUserCommand(request.Name, request.Email, request.OrganizationId), ct);
        return Results.Created($"/api/users/{created.Id}", created);
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var user = await mediator.Send(new GetUserByIdQuery(id), ct);
        return user is null ? Results.NotFound() : Results.Ok(user);
    }

    private static async Task<IResult> GetList(Guid? organizationId, string? search, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUsersListQuery(organizationId, search), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateUser(Guid id, UpdateUserRequest request, IMediator mediator, CancellationToken ct)
    {
        var updated = await mediator.Send(
            new UpdateUserCommand(id, request.Name, request.Email, request.OrganizationId, request.RoleIds), ct);
        return Results.Ok(updated);
    }

    private static async Task<IResult> DeleteUser(Guid id, IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new DeleteUserCommand(id), ct);
        return Results.NoContent();
    }
}
