using System.Collections.Immutable;
using Blog.Api.Contracts.Posts;
using Blog.Api.Database;
using Blog.Api.Shared;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Features.Posts;

public static class GetPosts
{
    public class Query : IRequest<Result<ICollection<PostDetailResponse>>>
    {
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Title { get; set; }
    }

    internal sealed class Handler(DataContext dbContext)
        : IRequestHandler<Query, Result<ICollection<PostDetailResponse>>>
    {
        public async Task<Result<ICollection<PostDetailResponse>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var itemsToSkip = (request.PageNo - 1) * request.PageSize;
            var response = await dbContext.Posts.AsNoTracking()
                .Skip(itemsToSkip)
                .Take(request.PageSize)
                .Where(p => EF.Functions.Like(p.Title, $"%{request.Title}%"))
                .ProjectToType<PostDetailResponse>().ToArrayAsync(cancellationToken: cancellationToken);

            return response;
        }
    }
}

public class GetPostsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/posts",
            async (ISender sender,
                CancellationToken cancellationToken = default, int page = 1, int size = 10, string? title = null) =>
            {
                var query = new GetPosts.Query() { PageNo = page, PageSize = size, Title = title };
                var result = await sender.Send(query, cancellationToken);
                return Results.Ok(result.Value);
            });
    }
}