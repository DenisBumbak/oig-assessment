using MediatR;
using OIG.Application.Features.Organizations.Create;
using OIG.Application.Features.Organizations.Delete;
using OIG.Application.Features.Organizations.GetById;
using OIG.Application.Features.Organizations.GetHierarchy;
using OIG.Application.Features.Organizations.Update;
using OIG.Domain.Roles;
using OIG.WebApi.Auth;

namespace OIG.WebApi.Endpoints.Organizations;

public static class OrganizationsEndpoints
{
    public static RouteGroupBuilder MapOrganizationsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/organizations").WithTags("Organizations");

        group.MapPost("/", CreateOrg)
            .WithName("CreateOrganization")
            .Produces<CreateOrganizationResponse>(StatusCodes.Status201Created)
            .RequirePermission(Permission.EditOrganization);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetOrganizationById")
            .Produces<GetOrganizationByIdResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(Permission.ViewOrganizations);

        group.MapGet("/hierarchy", GetHierarchy)
            .WithName("GetOrganizationHierarchy")
            .Produces<GetOrganizationHierarchyResponse>()
            .RequirePermission(Permission.ViewOrganizations);

        group.MapPut("/{id:guid}", UpdateOrg)
            .WithName("UpdateOrganization")
            .Produces<UpdateOrganizationResponse>()
            .RequirePermission(Permission.EditOrganization);

        group.MapDelete("/{id:guid}", DeleteOrg)
            .WithName("DeleteOrganization")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequirePermission(Permission.EditOrganization);

        return group;
    }

    private static async Task<IResult> CreateOrg(CreateOrganizationRequest request, IMediator mediator, CancellationToken ct)
    {
        var created = await mediator.Send(new CreateOrganizationCommand(request.Name, request.ParentId), ct);
        return Results.Created($"/api/organizations/{created.Id}", created);
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var org = await mediator.Send(new GetOrganizationByIdQuery(id), ct);
        return org is null ? Results.NotFound() : Results.Ok(org);
    }

    private static async Task<IResult> GetHierarchy(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrganizationHierarchyQuery(), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateOrg(Guid id, UpdateOrganizationRequest request, IMediator mediator, CancellationToken ct)
    {
        var updated = await mediator.Send(new UpdateOrganizationCommand(id, request.Name, request.ParentId), ct);
        return Results.Ok(updated);
    }

    private static async Task<IResult> DeleteOrg(Guid id, IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new DeleteOrganizationCommand(id), ct);
        return Results.NoContent();
    }
}
