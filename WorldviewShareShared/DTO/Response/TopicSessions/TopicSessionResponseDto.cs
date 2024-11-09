namespace WorldviewShareServer.Dtos;

public record TopicSessionResponseDto(Guid Id, string Name, string Topic, List<Guid> Users);