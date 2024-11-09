namespace WorldviewShareServer.Models;

public class TopicSession
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Topic { get; set; } = "";
    public List<User> Users { get; set; } = [];
    public List<Message> Messages { get; set; } = [];
}