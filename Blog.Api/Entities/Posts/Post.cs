using Blog.Api.Entities.Users;

namespace Blog.Api.Entities.Posts;

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }

    // Foreign key for Users
    public string UserId { get; set; }

    // Navigation property for Users
    public User User { get; set; }
}