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
        try
        {
            var databaseCreator = (Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator);
            databaseCreator!.CreateTables();
        }
        catch (SqliteException)
        {
            //A SqlException will be thrown if tables already exist. So simply ignore it.
        }
    }
    
    public DbSet<TopicSession> TopicSessions { get; set; }
    public DbSet<User> Users { get; set; }
}