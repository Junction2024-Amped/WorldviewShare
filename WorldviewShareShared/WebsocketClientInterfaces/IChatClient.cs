using WorldviewShareShared.DTO.Response.Messages;
namespace WorldviewShareShared.WebsocketClientInterfaces;

public interface IChatClient
{
    public Task ReceiveMessage(MessageResponseDto message);
}