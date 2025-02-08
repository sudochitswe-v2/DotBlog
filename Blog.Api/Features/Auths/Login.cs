using Blog.Api.Contracts.Auth;
using Blog.Api.Contracts.Users;
using Blog.Api.Database;
using Blog.Api.Entities.Users;
using Blog.Api.Security;
using Blog.Api.Shared;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace Blog.Api.Features.Auths;

public static class Login
{
    public class Query : IRequest<Result<AuthResponse>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    internal sealed class Handler(IConfiguration configuration, IJwtService jwtService, UserManager<User> userManager)
        : IRequestHandler<Query, Result<AuthResponse>>
    {
        public async Task<Result<AuthResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Result.Failure<AuthResponse>(new Error("User.NotFound", "User Not Found"));
            }

            if (!await userManager.CheckPasswordAsync(user, request.Password))
            {
                return Result.Failure<AuthResponse>(new Error("Password.Incorrect", "Password Incorrect"));
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

public class CreateUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/auth/login", async (LoginRequest request, ISender sender) =>
        {
            var query = request.Adapt<Login.Query>();

            var result = await sender.Send(query);

            return result.IsFailure ? Results.BadRequest(Result.Failure(result.Error)) : Results.Ok(result.Value);
        });
    }
}