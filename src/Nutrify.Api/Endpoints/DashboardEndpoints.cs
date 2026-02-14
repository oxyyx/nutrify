using System.Security.Claims;
using Nutrify.Api.Services;

namespace Nutrify.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/dashboard")
            .RequireAuthorization()
            .WithTags("Dashboard");

        group.MapGet("/today", async (IDashboardService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var dashboard = await service.GetTodayAsync(userId);
            return Results.Ok(dashboard);
        });

        group.MapGet("/weekly", async (IDashboardService service, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue("preferred_username")!;
            var overview = await service.GetWeeklyAsync(userId);
            return Results.Ok(overview);
        });

        return routes;
    }
}
