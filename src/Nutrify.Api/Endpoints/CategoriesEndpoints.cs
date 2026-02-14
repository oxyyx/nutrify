using System.Security.Claims;
using Nutrify.Api.Services;
using Nutrify.Contracts.Categories;

namespace Nutrify.Api.Endpoints;

public static class CategoriesEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/categories")
            .RequireAuthorization()
            .WithTags("Categories");

        group.MapGet("/", async (string? search, ICategoryService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var categories = await service.GetAllAsync(userId, search);
            return Results.Ok(categories);
        });

        group.MapGet("/{id:int}", async (int id, ICategoryService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var category = await service.GetByIdAsync(id, userId);
            return category is not null ? Results.Ok(category) : Results.NotFound();
        });

        group.MapPost("/", async (CreateCategoryRequest request, ICategoryService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var category = await service.CreateAsync(userId, request);
            return Results.Created($"/api/categories/{category.Id}", category);
        });

        group.MapPut("/{id:int}", async (int id, UpdateCategoryRequest request, ICategoryService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var category = await service.UpdateAsync(id, userId, request);
            return category is not null ? Results.Ok(category) : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async (int id, ICategoryService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var deleted = await service.DeleteAsync(id, userId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return routes;
    }
}
