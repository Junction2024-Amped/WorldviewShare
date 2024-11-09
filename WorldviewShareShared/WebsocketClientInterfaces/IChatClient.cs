using WorldviewShareShared.DTO.Response.Messages;
using WorldviewShareShared.DTO.Response.Users;
namespace WorldviewShareShared.WebsocketClientInterfaces;

public interface IChatClient
{
    public Task AcceptRegistration();
    public Task AcceptJoinSession(UserResponseDto activeUser);
    public Task AcceptLeaveSession();
    public Task RejectRegistration(string reason);
    public Task RejectJoinSession(string reason);
    public Task RejectLeaveSession(string reason);
    public Task RejectMessage(string reason);
    public Task ReceiveMessage(MessageResponseDto message);
    public Task ChangeActiveUser(UserResponseDto activeUser);
}