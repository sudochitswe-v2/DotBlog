using Blog.Api.Entities.Posts;
using Blog.Api.Entities.Users;
using Marques.EFCore.SnakeCase;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Database;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new PostConfiguration());
        modelBuilder.ToSnakeCase();
    }
}