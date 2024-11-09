using System.Text.Json.Serialization;

namespace WorldviewShareShared.DTO.Users.Create;

public class CreateUserDto
{
    [JsonPropertyName("username")] public string Username { get; set; }

    [JsonPropertyName("id")] public Guid Id { get; set; }
}