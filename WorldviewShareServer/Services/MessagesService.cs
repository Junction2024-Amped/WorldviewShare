using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Data;
using WorldviewShareServer.Models;
using WorldviewShareShared.DTO.Request.Messages;
using WorldviewShareShared.DTO.Response.Messages;
namespace WorldviewShareServer.Services;

public class MessagesService
{
    private readonly WorldviewShareContext _context;

    public MessagesService(WorldviewShareContext context)
    {
        _context = context;
    }
    
    public Message ToMessage(MessageRequestDto messageDto)
    {
        var message = new Message
        {
            Content = messageDto.Content,
            TopicSessionId = messageDto.TopicSessionId,
            AuthorId = messageDto.AuthorId,
            CreatedAt = DateTime.Now
        };
        _context.Messages.Add(message);
        return message;
    }
    
    public async Task<List<Message>> GetMessages() => await _context.Messages.ToListAsync();
        
    public async Task<Message?> GetMessageById(Guid id) => await _context.Messages.FindAsync(id);
        
    public MessageResponseDto ToMessageResponseDto(Message message) => new(message.Id, message.Content, message.TopicSessionId, message.AuthorId, message.CreatedAt);
    
    public void RemoveMessage(Message message) => _context.Messages.Remove(message);
    
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    
    public void SetEntityState(Message message, EntityState state) => _context.Entry(message).State = state;

    public bool MessageExists(Guid id) => _context.Messages.Any(e => e.Id == id);
}