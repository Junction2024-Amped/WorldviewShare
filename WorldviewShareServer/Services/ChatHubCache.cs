using WorldviewShareServer.Models;

namespace WorldviewShareServer.Services;

public class ChatHubCache
{
    public readonly Dictionary<TopicSession, User> ActiveUsers = new();
    public readonly Dictionary<string, User> Users = new();
}