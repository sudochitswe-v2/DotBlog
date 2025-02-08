using System.Security.Claims;
using Blog.Api.Contracts.Auth;
using Blog.Api.Contracts.Users;
using Blog.Api.Entities.Users;
using Blog.Api.Security;
using Blog.Api.Shared;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Blog.Api.Features.Auths;

public static class TokenRefresh
{
    public class Command : IRequest<Result<AuthResponse>>
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    internal sealed class Handler(IConfiguration configuration, IJwtService jwtService, UserManager<User> userManager)
        : IRequestHandler<Command, Result<AuthResponse>>
    {
        public async Task<Result<AuthResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var principal = jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal is null)
            {
                return Result.Failure<AuthResponse>(new Error("Unauthorized", "Access token is invalid"));
            }

            var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
            {
                return Result.Failure<AuthResponse>(new Error("Unauthorized", "Access token is invalid"));
            }

            if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Result.Failure<AuthResponse>(new Error("Unauthorized", "Refresh token is invalid"));
            }

            var accessToken = jwtService.GenerateJwtToken(user);
            var refreshToken = jwtService.GenerateRefreshToken();
            var jwtSettings = configuration.GetSection("JwtSettings");
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime =
                DateTime.UtcNow.AddDays(int.Parse(jwtSettings["RefreshTokenExpirationDays"]));
            await userManager.UpdateAsync(user);
            var response = new AuthResponse
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RefreshToken = refreshToken,
                AccessToken = accessToken,
            };
            return Result.Success(response);
        }
    }
}

public class TokenRefreshEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/v1/auth/tokens",
            async (RefreshTokenRequest model, ISender sender, CancellationToken cancellationToken = default) =>
            {
                var command = model.Adapt<TokenRefresh.Command>();
                var result = await sender.Send(command, cancellationToken);

                return result.IsFailure ? Results.Unauthorized() : Results.Ok(result.Value);
            });
    }
}