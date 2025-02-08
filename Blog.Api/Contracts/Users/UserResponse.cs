using System.Text.Json.Serialization;

namespace Blog.Api.Contracts.Users;

public class UserResponse
{
    public int UserId { get; set; }
    [JsonIgnore] public string FirstName { get; set; }
    [JsonIgnore] public string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}