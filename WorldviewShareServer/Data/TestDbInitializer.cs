using WorldviewShareServer.Models;
namespace WorldviewShareServer.Data;

public static class TestDbInitializer
{
    // TODO: Figure out how to call this
    public static void Initialize(WorldviewShareContext context)
    {
        context.Database.EnsureCreated();

        if (context.TopicSessions.Any())
        {
            return;
        }

        var topicSessions = new[]
        {
            new TopicSession
            {
                Name = "Test Session",
                Topic = "Test Topic",
                Users = new List<User>
                {
                    new() {Username = "Test User 1"},
                    new() {Username = "Test User 2"}
                }
            }
        };
        
        context.TopicSessions.AddRange(topicSessions);
        context.SaveChanges();
    }
}