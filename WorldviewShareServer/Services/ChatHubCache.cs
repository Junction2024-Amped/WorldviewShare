using WorldviewShareServer.Models;

namespace WorldviewShareServer.Services;

public class ChatHubCache
{
    public readonly Dictionary<TopicSession, User> _activeUsers = new();
    public readonly Dictionary<string, User> _users = new();
}