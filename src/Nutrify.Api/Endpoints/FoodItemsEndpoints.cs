using System.Security.Claims;
using Nutrify.Api.Services;
using Nutrify.Contracts.Common;
using Nutrify.Contracts.FoodItems;

namespace Nutrify.Api.Endpoints;

public static class FoodItemsEndpoints
{
    public static IEndpointRouteBuilder MapFoodItemEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/food-items")
            .RequireAuthorization()
            .WithTags("Food Items");

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            string? search,
            int? categoryId,
            FoodItemType? type,
            IFoodItemService service,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var pagination = new PaginationRequest(page ?? 1, pageSize ?? 20);
            var result = await service.GetAllAsync(userId, pagination, search, categoryId, type);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, IFoodItemService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var foodItem = await service.GetByIdAsync(id, userId);
            return foodItem is not null ? Results.Ok(foodItem) : Results.NotFound();
        });

        group.MapPost("/", async (CreateFoodItemRequest request, IFoodItemService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var foodItem = await service.CreateAsync(userId, request);
            return Results.Created($"/api/food-items/{foodItem.Id}", foodItem);
        });

        group.MapPut("/{id:int}", async (int id, UpdateFoodItemRequest request, IFoodItemService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var foodItem = await service.UpdateAsync(id, userId, request);
            return foodItem is not null ? Results.Ok(foodItem) : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, IFoodItemService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var deleted = await service.DeleteAsync(id, userId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return routes;
    }
}
