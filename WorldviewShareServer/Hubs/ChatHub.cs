using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using WorldviewShareServer.Models;
using WorldviewShareServer.Services;
using WorldviewShareShared.DTO.Request.Messages;
namespace WorldviewShareServer.Hubs;

[SignalRHub("messages")]
public class ChatHub : Hub
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
        await Clients.All.SendAsync("ReceiveMessage", _service.ToMessageResponseDto(message));
    }
}