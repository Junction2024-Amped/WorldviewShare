namespace WorldviewShareShared.DTO.Response.Messages;

public record MessageResponseDto(Guid Id, string Content, Guid TopicSessionId, Guid AuthorId, DateTime CreatedAt);