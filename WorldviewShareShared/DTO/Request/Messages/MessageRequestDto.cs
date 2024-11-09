namespace WorldviewShareServer.Dtos;

public record MessageRequestDto(string Content, Guid TopicSessionId, Guid AuthorId);