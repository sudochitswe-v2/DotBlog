namespace Blog.Api.Contracts.Users;

public class CreateUserResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}