using System.Text.Json.Serialization;

namespace WorldviewShareShared.DTO.Users.Get;

public class GetUserDto
{
    [JsonPropertyName("username")] public string Username { get; set; }

    [JsonPropertyName("id")] public Guid Id { get; set; }
}