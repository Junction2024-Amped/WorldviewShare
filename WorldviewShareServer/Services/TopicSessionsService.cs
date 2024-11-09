using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Data;
using WorldviewShareServer.Models;
using WorldviewShareShared.DTO.Request.TopicSessions;
using WorldviewShareShared.DTO.Response.TopicSessions;
namespace WorldviewShareServer.Services;

public class TopicSessionsService
{
    private readonly WorldviewShareContext _context;
    
    public TopicSessionsService(WorldviewShareContext context)
    {
        _context = context;
    }
    
    public TopicSession ToTopicSession(TopicSessionRequestDto topicSessionRequestDto)
    {
        var topicSession = new TopicSession
        {
            Name = topicSessionRequestDto.Name,
            Topic = topicSessionRequestDto.Topic
        };
        _context.TopicSessions.Add(topicSession);
        return topicSession;
    }
    
    public async Task<List<TopicSession>> GetTopicSessions() => await _context.TopicSessions.ToListAsync();
        
    public async Task<TopicSession?> GetTopicSessionById(Guid id) => await _context.TopicSessions.FindAsync(id);
        
    public TopicSessionResponseDto ToTopicSessionResponseDto(TopicSession topicSession) => new(topicSession.Id, topicSession.Name, topicSession.Topic, topicSession.Users.Select(u => u.Id).ToList());
    
    public bool TopicSessionExists(Guid id) => _context.TopicSessions.Any(e => e.Id == id);
        
    public void RemoveTopicSession(TopicSession topicSession) => _context.TopicSessions.Remove(topicSession);
    
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    
    public void SetEntityState(TopicSession topicSession, EntityState state) => _context.Entry(topicSession).State = state;
}