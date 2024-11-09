namespace WorldviewShareShared.DTO.Response.Users;

public record UserResponseDto(Guid Id, string Username, List<Guid> TopicSessions);