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
    private readonly ChatHubCache _chatHubCache;
    private readonly MessagesService _messagesService;
    private readonly TopicSessionsService _topicSessionsService;
    private readonly UsersService _usersService;

    public ChatHub(
        MessagesService messagesService, TopicSessionsService topicSessionsService,
        UsersService usersService, ChatHubCache chatHubCache
    )
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

        _chatHubCache.ActiveUsers.TryAdd(topicSession.Id, user);
        await Groups.AddToGroupAsync(Context.ConnectionId, topicSession.Id.ToString());
        await Clients.Caller.AcceptJoinSession(
            _usersService.ToUserResponseDto(_chatHubCache.ActiveUsers[topicSession.Id]));

        if (!_chatHubCache.TopicsPerUser.TryAdd(user.Id, [topicSession.Id]))
            _chatHubCache.TopicsPerUser[user.Id].Add(topicSession.Id);
    }

    public async Task LeaveSession()
    {
        if (!_chatHubCache.Users.TryGetValue(Context.ConnectionId, out var user))
        {
            await Clients.Caller.RejectLeaveSession("User not registered");

            return;
        }

        var successfulRemoval = false;
        foreach (var (userId, topicIds) in _chatHubCache.TopicsPerUser.ToList())
        {
            if (userId == user.Id)
            {
                successfulRemoval = true;
                foreach (var topicId in topicIds)
                {
                    if (_chatHubCache.ActiveUsers.TryGetValue(topicId, out var activeUser))
                    {
                        if (activeUser.Id == user.Id)
                        {
                            var newActiveUser = _chatHubCache.TopicsPerUser.Where(kv => kv.Value.Contains(topicId)).Select(async kv => await _usersService.GetUserById(kv.Key)).Select(t => t.GetAwaiter().GetResult()).Cast<User>().Order(new RandomComparer<User>()).FirstOrDefault(u => u.Id != user.Id);
                            if (newActiveUser != null)
                            {
                                _chatHubCache.ActiveUsers[topicId] = newActiveUser;
                                await Clients.Group(topicId.ToString()).ChangeActiveUser(_usersService.ToUserResponseDto(_chatHubCache.ActiveUsers[topicId]));
                            }
                            else
                            {
                                _chatHubCache.ActiveUsers.Remove(topicId);
                            }

                        }
                    }
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, topicId.ToString());
                }
                _chatHubCache.TopicsPerUser.Remove(userId);
            }
        }

        if (successfulRemoval)
        {
            await Clients.Caller.AcceptLeaveSession();
        }
        else
        {
            await Clients.Caller.RejectLeaveSession("User not in a session");
        }
    }

    public async Task SendMessage(MessageRequestDto messageDto)
    {
        var message = _messagesService.ToMessage(messageDto);
        message.TopicSession = (await _topicSessionsService.GetTopicSessionById(messageDto.TopicSessionId))!;
        message.Author = (await _usersService.GetUserById(messageDto.AuthorId))!;
        await _messagesService.SaveChangesAsync();
        if (!_chatHubCache.Users.TryGetValue(Context.ConnectionId, out _))
            _chatHubCache.Users[Context.ConnectionId] = message.Author;

        if (_chatHubCache.ActiveUsers.TryGetValue(message.TopicSession.Id, out var activeUser))
            if (activeUser.Id != message.Author.Id)
            {
                await Clients.Caller.RejectMessage("User is not active user in session");

                return;
            }

        var newActiveUser = _chatHubCache.TopicsPerUser.Where(kv => kv.Value.Contains(message.TopicSessionId)).Select(async kv => await _usersService.GetUserById(kv.Key)).Select(t => t.GetAwaiter().GetResult()).Cast<User>().Order(new RandomComparer<User>()).FirstOrDefault(u => u.Id != message.Author.Id);
        if (newActiveUser != null)
        {
            _chatHubCache.ActiveUsers[message.TopicSession.Id] = newActiveUser;
            await Clients.Group(message.TopicSessionId.ToString()).ChangeActiveUser(_usersService.ToUserResponseDto(newActiveUser));
        }
        else
        {
            await Clients.Group(message.TopicSessionId.ToString()).ChangeActiveUser(_usersService.ToUserResponseDto(_chatHubCache.ActiveUsers[message.TopicSession.Id]));
        }

        await Clients.Group(message.TopicSessionId.ToString()).ReceiveMessage(_messagesService.ToMessageResponseDto(message));
    }

    [SignalRHidden]
    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        if (_chatHubCache.Users.Remove(Context.ConnectionId, out var user))
        {
            foreach (var (userId, topicIds) in _chatHubCache.TopicsPerUser.ToList())
            {
                if (userId == user.Id)
                {
                    foreach (var topicId in topicIds)
                    {
                        if (_chatHubCache.ActiveUsers.TryGetValue(topicId, out var activeUser))
                        {
                            if (activeUser.Id == user.Id)
                            {
                                var newActiveUser = _chatHubCache.TopicsPerUser.Where(kv => kv.Value.Contains(topicId)).Select(async kv => await _usersService.GetUserById(kv.Key)).Select(t => t.GetAwaiter().GetResult()).Cast<User>().Order(new RandomComparer<User>()).FirstOrDefault(u => u.Id != user.Id);
                                if (newActiveUser != null)
                                {
                                    _chatHubCache.ActiveUsers[topicId] = newActiveUser;
                                    await Clients.Group(topicId.ToString()).ChangeActiveUser(_usersService.ToUserResponseDto(_chatHubCache.ActiveUsers[topicId]));
                                }
                                else
                                {
                                    _chatHubCache.ActiveUsers.Remove(topicId);
                                }

                            }
                        }
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, topicId.ToString());
                    }
                    _chatHubCache.TopicsPerUser.Remove(userId);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}