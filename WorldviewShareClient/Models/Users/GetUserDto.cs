using System;
using System.Text.Json.Serialization;

namespace WorldviewShareClient.Models.Users;

public class GetUserDto
{
    [JsonPropertyName("username")] public string Username { get; set; }

    [JsonPropertyName("id")] public Guid Id { get; set; }
}