using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using WorldviewShareServer.Models;
namespace WorldviewShareServer.Data;

public sealed class WorldviewShareContext : DbContext
{
    public WorldviewShareContext(DbContextOptions<WorldviewShareContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    public DbSet<TopicSession> TopicSessions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TopicSession>().HasMany(ts => ts.Users).WithMany(u => u.TopicSessions).UsingEntity(j => j.ToTable("UserTopicSessions"));
        modelBuilder.Entity<TopicSession>().HasMany(ts => ts.Messages).WithOne(m => m.TopicSession).HasForeignKey(m => m.TopicSessionId);
        modelBuilder.Entity<User>().HasMany(u => u.Messages).WithOne(m => m.Author).HasForeignKey(m => m.AuthorId);
    }
}