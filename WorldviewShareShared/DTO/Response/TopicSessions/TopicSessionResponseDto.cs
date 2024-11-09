namespace WorldviewShareShared.DTO.Response.TopicSessions;

public record TopicSessionResponseDto(Guid Id, string Name, string Topic, List<Guid> Users);