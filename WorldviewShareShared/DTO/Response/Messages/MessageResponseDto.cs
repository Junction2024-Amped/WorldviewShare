namespace WorldviewShareShared.DTO.Response.Messages;

public record MessageResponseDto(Guid Id, string Content, Uri? Source, Guid TopicSessionId, Guid AuthorId, DateTime CreatedAt);