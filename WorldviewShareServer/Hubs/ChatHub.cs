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
        if (user == null)
        {
            await Clients.Caller.RejectRegistration("User not found");
            return;
        }
        _users[Context.ConnectionId] = user;
        await Clients.Caller.AcceptRegistration();
    }
    
    public async Task JoinSession(TopicSessionReferenceRequestDto request)
    {
        var topicSession = await _topicSessionsService.GetTopicSessionById(request.Id);
        if (topicSession == null)
        {
            await Clients.Caller.RejectJoinSession("Topic session not found");
            return;
        }
        if (!_users.TryGetValue(Context.ConnectionId, out var user))
        {
            await Clients.Caller.RejectJoinSession("User not registered");
            return;
        }
        _activeUsers[topicSession] = user;
        await Groups.AddToGroupAsync(Context.ConnectionId, topicSession.Id.ToString());
        await Clients.Caller.AcceptJoinSession();
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