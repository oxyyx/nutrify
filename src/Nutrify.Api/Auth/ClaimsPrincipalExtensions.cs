using System.Security.Claims;

namespace Nutrify.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the stable Keycloak subject id ("sub" claim) for the
    /// authenticated user. Usernames are mutable and reusable, so per-user
    /// data must be keyed by subject instead. The default JWT handler maps
    /// "sub" to <see cref="ClaimTypes.NameIdentifier"/>, so check both.
    /// </summary>
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? user.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("Token contains no subject claim.");
}
