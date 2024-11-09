namespace WorldviewShareShared.DTO.Request.Messages;

public record MessageRequestDto(string Content, Uri? Source, Guid TopicSessionId, Guid AuthorId);