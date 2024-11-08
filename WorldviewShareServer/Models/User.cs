namespace WorldviewShareServer.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public List<TopicSession> TopicSessions { get; set; } = [];
}