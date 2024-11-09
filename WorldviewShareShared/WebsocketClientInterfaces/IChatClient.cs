using WorldviewShareShared.DTO.Response.Messages;
namespace WorldviewShareShared.WebsocketClientInterfaces;

public interface IChatClient
{
    public Task AcceptRegistration();
    public Task AcceptJoinSession();
    public Task AcceptLeaveSession();
    public Task RejectRegistration(string reason);
    public Task RejectJoinSession(string reason);
    public Task RejectLeaveSession(string reason);
    public Task ReceiveMessage(MessageResponseDto message);
}