using Blog.Api.Contracts.Users;

namespace Blog.Api.Contracts.Posts;

public class CreatePostResponse
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; }

// Navigation property for Users
    public UserResponse User { get; set; }
}