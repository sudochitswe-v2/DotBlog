using Blog.Api.Contracts.Posts;
using Blog.Api.Database;
using Blog.Api.Shared;
using Carter;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Features.Posts;

public static class GetPostDetails
{
    public class Query : IRequest<Result<PostDetailResponse>>
    {
        public int PostId { get; set; }
    }

    internal sealed class Handler(DataContext dbContext)
        : IRequestHandler<Query, Result<PostDetailResponse>>
    {
        public async Task<Result<PostDetailResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var response = await dbContext.Posts.AsNoTracking()
                .ProjectToType<PostDetailResponse>()
                .FirstOrDefaultAsync(e => e.PostId == request.PostId, cancellationToken: cancellationToken);

            if (response == null)
            {
                return Result.Failure<PostDetailResponse>(new Error(
                    "GetPost.NotFound",
                    "Posts not found."));
            }

            return response;
        }
    }
}

public class GetPostDetailsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/posts/{id}/details",
            async (int id, ISender sender, CancellationToken cancellationToken = default) =>
            {
                var query = new GetPostDetails.Query { PostId = id };
                var result = await sender.Send(query, cancellationToken);
                return result.IsFailure ? Results.NoContent() : Results.Ok(result.Value);
            });
    }
}