namespace WorldviewShareServer.Dtos;

public record MessageResponseDto(Guid Id, string Content, Guid TopicSessionId, Guid AuthorId, DateTime CreatedAt);