namespace WorldviewShareServer.Models;

public class TopicSession
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Topic { get; set; }
    public ICollection<User> Users { get; set; }
}