using Blog.Api.Entities.Posts;
using Microsoft.AspNetCore.Identity;

namespace Blog.Api.Entities.Users;

public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property for Posts
    public ICollection<Post> Posts { get; set; }
}