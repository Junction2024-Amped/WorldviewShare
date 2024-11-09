using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using WorldviewShareServer.Models;
using WorldviewShareServer.Services;
using WorldviewShareShared.DTO.Request.Messages;
using WorldviewShareShared.DTO.Request.TopicSessions;
using WorldviewShareShared.DTO.Request.Users;
using WorldviewShareShared.WebsocketClientInterfaces;
namespace WorldviewShareServer.Hubs;

[SignalRHub("messages")]
public class ChatHub : Hub<IChatClient>
{
    private readonly MessagesService _messagesService;
    private readonly TopicSessionsService _topicSessionsService;
    private readonly UsersService _usersService;
    private readonly Dictionary<TopicSession, User> _activeUsers = new();
    private readonly Dictionary<string, User> _users = new();
    public ChatHub(MessagesService messagesService, TopicSessionsService topicSessionsService, UsersService usersService)
    {
        _messagesService = messagesService;
        _topicSessionsService = topicSessionsService;
        _usersService = usersService;
    }
    
    public async Task Register(UserReferenceRequestDto userDto)
    {
        var user = await _usersService.GetUserById(userDto.Id);
        _users[Context.ConnectionId] = user ?? throw new ArgumentException("User not found"); // TODO: make this handling less horrible
    }
    
    public async Task JoinSession(TopicSessionReferenceRequestDto request)
    {
        var topicSession = await _topicSessionsService.GetTopicSessionById(request.Id);
        if (topicSession == null)
        {
            throw new ArgumentException("Topic session not found"); // TODO: make this handling less horrible
        }
        if (!_users.TryGetValue(Context.ConnectionId, out var user))
        {
            throw new ArgumentException("User not found"); // TODO: make this handling less horrible
        }
        _activeUsers[topicSession] = user;
        await Groups.AddToGroupAsync(Context.ConnectionId, topicSession.Id.ToString());
    }

    public async Task SendMessage(MessageRequestDto messageDto)
    {
        var message = _messagesService.ToMessage(messageDto);
        await _messagesService.SaveChangesAsync();
        await Clients.All.ReceiveMessage(_messagesService.ToMessageResponseDto(message));
    }
    
    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        if (_users.Remove(Context.ConnectionId, out var user))
        {
            foreach (var (topicSession, activeUser) in _activeUsers)
            {
                if (activeUser == user)
                {
                    _activeUsers.Remove(topicSession);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, topicSession.Id.ToString());
                }
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}