namespace WorldviewShareShared.DTO.Request.Messages;

public record MessageRequestDto(string Content, Guid TopicSessionId, Guid AuthorId);