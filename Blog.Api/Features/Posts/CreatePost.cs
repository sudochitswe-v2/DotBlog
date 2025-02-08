using Blog.Api.Contracts.Posts;
using Blog.Api.Database;
using Blog.Api.Entities.Posts;
using Blog.Api.Entities.Users;
using Blog.Api.Extensions;
using Blog.Api.Shared;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Features.Posts;

public static class CreatePost
{
    public class Command : IRequest<Result<CreatePostResponse>>
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Content).NotEmpty();
        }
    }

    internal sealed class Handler(DataContext dbContext, IValidator<Command> validator)
        : IRequestHandler<Command, Result<CreatePostResponse>>
    {
        public async Task<Result<CreatePostResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var post = new Post
            {
                Title = request.Title,
                Content = request.Content,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Add(post);

            await dbContext.SaveChangesAsync(cancellationToken);

            var response = await dbContext.Posts.AsNoTracking()
                .ProjectToType<CreatePostResponse>()
                .FirstAsync(p => p.PostId == post.PostId, cancellationToken: cancellationToken);

            return response;
        }
    }
}

public class CreatePostEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/posts", async (HttpContext context, CreatePostRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreatePost.Command>();
            command.UserId = context.User.UniqueId()!;
            var result = await sender.Send(command);
            return Results.Ok(result.Value);
        }).RequireAuthorization();
    }
}