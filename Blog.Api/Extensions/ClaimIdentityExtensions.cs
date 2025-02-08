using System.Security.Claims;

namespace Blog.Api.Extensions;

public static class ClaimIdentityExtensions
{
    public static string? UniqueId(this ClaimsPrincipal claimsPrincipal)
    {
        var id = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        return id;
    }
}