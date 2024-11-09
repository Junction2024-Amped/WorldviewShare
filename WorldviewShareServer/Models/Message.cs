using System.ComponentModel.DataAnnotations;
namespace WorldviewShareServer.Models;

public class Message
{
    public Guid Id { get; set; }
    [MaxLength(256)]
    public string Content { get; set; } = "";
    public User Author { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public TopicSession TopicSession { get; set; } = null!;
    public Guid TopicSessionId { get; set; }
    public DateTime CreatedAt { get; set; }
}