namespace WorldviewShareServer.Dtos;

public record UserResponseDto(Guid Id, string Username, List<Guid> TopicSessions);