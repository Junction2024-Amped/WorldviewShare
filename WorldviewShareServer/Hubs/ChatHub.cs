using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using WorldviewShareServer.Models;
using WorldviewShareServer.Services;
using WorldviewShareShared.DTO.Request.Messages;
using WorldviewShareShared.WebsocketClientInterfaces;
namespace WorldviewShareServer.Hubs;

[SignalRHub("messages")]
public class ChatHub : Hub<IChatClient>
{
    private readonly MessagesService _service;
    private readonly Dictionary<TopicSession, User> _activeUsers = new();
    public ChatHub(MessagesService service)
    {
        _service = service;
    }
    
    public async Task SendMessage(MessageRequestDto messageDto)
    {
        var message = _service.ToMessage(messageDto);
        await _service.SaveChangesAsync();
        await Clients.All.ReceiveMessage(_service.ToMessageResponseDto(message));
    }
}