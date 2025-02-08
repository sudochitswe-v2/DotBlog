using Carter;

namespace Blog.Api.Features.Auths;

public static class AuthTest
{
}

public class AuthTestEndPost : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/auth/test", () => Results.Ok()).RequireAuthorization();
    }
}