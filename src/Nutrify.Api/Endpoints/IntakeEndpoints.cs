using System.Security.Claims;
using Nutrify.Api.Services;
using Nutrify.Contracts.Common;
using Nutrify.Contracts.Intake;

namespace Nutrify.Api.Endpoints;

public static class IntakeEndpoints
{
    public static IEndpointRouteBuilder MapIntakeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/intake")
            .RequireAuthorization()
            .WithTags("Intake");

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            DateOnly? date,
            DateOnly? from,
            DateOnly? to,
            IIntakeService service,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var pagination = new PaginationRequest(page ?? 1, pageSize ?? 20);
            var result = await service.GetEntriesAsync(userId, pagination, date, from, to);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, IIntakeService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var entry = await service.GetByIdAsync(id, userId);
            return entry is not null ? Results.Ok(entry) : Results.NotFound();
        });

        group.MapPost("/", async (CreateIntakeEntryRequest request, IIntakeService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var entry = await service.CreateAsync(userId, request);
            return Results.Created($"/api/intake/{entry.Id}", entry);
        });

        group.MapPut("/{id:int}", async (int id, UpdateIntakeEntryRequest request, IIntakeService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var entry = await service.UpdateAsync(id, userId, request);
            return entry is not null ? Results.Ok(entry) : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, IIntakeService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var deleted = await service.DeleteAsync(id, userId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return routes;
    }
}
