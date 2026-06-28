namespace Nutrify.Api.Endpoints;

/// <summary>
/// Exposes runtime configuration the SPA needs at startup. Because the client
/// is a static bundle baked into the image, anything environment-specific
/// (e.g. the OIDC issuer) must be delivered at runtime rather than build time.
/// </summary>
public static class ConfigEndpoints
{
    public static IEndpointRouteBuilder MapConfigEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/config", (IConfiguration config) =>
        {
            var authServerUrl = config["Keycloak:auth-server-url"]?.TrimEnd('/');
            var realm = config["Keycloak:realm"];

            // Prefer an explicit issuer override, otherwise derive it from the
            // Keycloak settings the API already uses.
            var issuer = config["Spa:OidcIssuer"]
                ?? (authServerUrl is not null && realm is not null
                    ? $"{authServerUrl}/realms/{realm}"
                    : null);

            var clientId = config["Spa:OidcClientId"] ?? "nutrify-spa";

            return Results.Ok(new SpaConfig(issuer, clientId));
        })
        .AllowAnonymous()
        .WithTags("Config");

        return routes;
    }

    private sealed record SpaConfig(string? OidcIssuer, string OidcClientId);
}
