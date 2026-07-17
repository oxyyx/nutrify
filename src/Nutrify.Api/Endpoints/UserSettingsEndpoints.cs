using System.Security.Claims;
using Nutrify.Api.Auth;
using Nutrify.Api.Services;
using Nutrify.Contracts.Settings;

namespace Nutrify.Api.Endpoints;

public static class UserSettingsEndpoints
{
    public static IEndpointRouteBuilder MapUserSettingsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/settings")
            .RequireAuthorization()
            .WithTags("Settings");

        group.MapGet("/", async (IUserSettingsService service, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var settings = await service.GetAsync(userId);
            return Results.Ok(settings);
        });

        group.MapPut("/", async (UpdateUserSettingsRequest request, IUserSettingsService service, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var settings = await service.UpdateAsync(userId, request);
            return Results.Ok(settings);
        });

        return routes;
    }
}
