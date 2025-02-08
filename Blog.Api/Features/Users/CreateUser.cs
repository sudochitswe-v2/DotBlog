using Blog.Api.Contracts;
using Blog.Api.Contracts.Users;
using Blog.Api.Database;
using Blog.Api.Entities.Users;
using Blog.Api.Shared;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Blog.Api.Features.Users;

public static class CreateUser
{
    public class Command : IRequest<Result<CreateUserResponse>>
    {
        public string FirtName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.FirtName).NotEmpty();
            RuleFor(c => c.LastName).NotEmpty();
            RuleFor(c => c.Email).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
        }
    }

    internal sealed class Handler(DataContext dbContext, UserManager<User> userManager, IValidator<Command> validator)
        : IRequestHandler<Command, Result<CreateUserResponse>>
    {
        public async Task<Result<CreateUserResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
            // if (!validationResult.IsValid)
            // {
            //     return Result.Failure<CreateUserResponse>(new Error(
            //         "CreateUser.Validation",
            //         validationResult.ToString()));
            // }

            var user = new User
            {
                FirstName = request.FirtName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return user.Adapt<CreateUserResponse>();
        }
    }
}

public class CreateUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/users", async (CreateUserRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateUser.Command>();

            var result = await sender.Send(command);

            return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result.Value);
        });
    }
}