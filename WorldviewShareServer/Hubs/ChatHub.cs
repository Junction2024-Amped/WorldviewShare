using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalRSwaggerGen.Attributes;
using WorldviewShareServer.Models;
using WorldviewShareServer.Services;
using WorldviewShareShared.DTO.Request.Messages;
using WorldviewShareShared.DTO.Request.TopicSessions;
using WorldviewShareShared.DTO.Request.Users;
using WorldviewShareShared.Utils;
using WorldviewShareShared.WebsocketClientInterfaces;
namespace WorldviewShareServer.Hubs;

[SignalRHub("messages")]
public class ChatHub : Hub<IChatClient>
{
    private readonly MessagesService _messagesService;
    private readonly TopicSessionsService _topicSessionsService;
    private readonly UsersService _usersService;
    private readonly ChatHubCache _chatHubCache;
    public ChatHub(MessagesService messagesService, TopicSessionsService topicSessionsService, UsersService usersService, ChatHubCache chatHubCache)
    {
        _messagesService = messagesService;
        _topicSessionsService = topicSessionsService;
        _usersService = usersService;
        _chatHubCache = chatHubCache;
    }
    
    public async Task Register(UserReferenceRequestDto userDto)
    {
        var user = await _usersService.GetUserById(userDto.Id);
        if (user == null)
        {
            await Clients.Caller.RejectRegistration("User not found");
            return;
        }
        _chatHubCache.Users[Context.ConnectionId] = user;
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
        if (!_chatHubCache.Users.TryGetValue(Context.ConnectionId, out var user))
        {
            await Clients.Caller.RejectJoinSession("User not registered");
            return;
        }
        if (topicSession.Users.Contains(user))
        {
            await Clients.Caller.RejectJoinSession("User already in session");
            return;
        }

        _chatHubCache.ActiveUsers.TryAdd(topicSession, user);
        await Groups.AddToGroupAsync(Context.ConnectionId, topicSession.Id.ToString());
        await Clients.Caller.AcceptJoinSession(_usersService.ToUserResponseDto(_chatHubCache.ActiveUsers[topicSession]));
        topicSession.Users.Add(user);
        _topicSessionsService.SetEntityState(topicSession, EntityState.Modified);
        await _topicSessionsService.SaveChangesAsync();
    }
    
    public async Task LeaveSession()
    {
        if (!_chatHubCache.Users.TryGetValue(Context.ConnectionId, out var user))
        {
            await Clients.Caller.RejectLeaveSession("User not registered");
            return;
        }
        foreach (var (topicSession, activeUser) in _chatHubCache.ActiveUsers)
        {
            if (activeUser == user)
            {
                _chatHubCache.ActiveUsers.Remove(topicSession);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, topicSession.Id.ToString());
                await Clients.Caller.AcceptLeaveSession();
                return;
            }
        }
        await Clients.Caller.RejectLeaveSession("User not in a session");
    }

    public async Task SendMessage(MessageRequestDto messageDto)
    {
        var message = _messagesService.ToMessage(messageDto);
        await _messagesService.SaveChangesAsync();
        if (!_chatHubCache.Users.TryGetValue(Context.ConnectionId, out _))
        {
            _chatHubCache.Users[Context.ConnectionId] = message.Author;
        }
        if (_chatHubCache.ActiveUsers.TryGetValue(message.TopicSession, out var activeUser))
        {
            if (activeUser != message.Author)
            {
                await Clients.Caller.RejectMessage("User is not active user in session");
                return;
            }
        }
        var newActiveUser = message.TopicSession.Users.Order(new RandomComparer<User>()).FirstOrDefault(u => u != message.Author);
        if (newActiveUser != null)
        {
            _chatHubCache.ActiveUsers[message.TopicSession] = newActiveUser;
            await Clients.Group(message.TopicSessionId.ToString()).ChangeActiveUser(_usersService.ToUserResponseDto(newActiveUser));
        }
        await Clients.Group(message.TopicSessionId.ToString()).ReceiveMessage(_messagesService.ToMessageResponseDto(message));
    }
    
    [SignalRHidden]
    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        if (_chatHubCache.Users.Remove(Context.ConnectionId, out var user))
        {
            foreach (var (topicSession, activeUser) in _chatHubCache.ActiveUsers)
            {
                if (activeUser == user)
                {
                    _chatHubCache.ActiveUsers.Remove(topicSession);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, topicSession.Id.ToString());
                }
            }
            user.TopicSessions.Clear();
            _usersService.SetEntityState(user, EntityState.Modified);
            await _usersService.SaveChangesAsync();
        }
        await base.OnDisconnectedAsync(exception);
    }
}