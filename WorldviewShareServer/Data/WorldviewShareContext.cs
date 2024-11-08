using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Models;
namespace WorldviewShareServer.Data;

public class WorldviewShareContext : DbContext
{
    public WorldviewShareContext(DbContextOptions<WorldviewShareContext> options) : base(options) { }
    
    public DbSet<TopicSession> TopicSessions { get; set; }
    public DbSet<User> Users { get; set; }
}