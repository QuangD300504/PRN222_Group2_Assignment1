namespace PRN222_Group2_Assignment1.ViewModels;

public class CitationDto
{
    public int Index { get; set; }
    public int ChunkId { get; set; }
    public string DocumentTitle { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public string? Heading { get; set; }
    public string Snippet { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public string Role { get; set; } = "user"; // "user" | "assistant"
    public string Content { get; set; } = string.Empty;
    public List<CitationDto> Citations { get; set; } = [];
    public List<string> SuggestedFollowUps { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public class ChatSessionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public List<int> SelectedDocumentIds { get; set; } = [];
    public DateTime UpdatedAt { get; set; }
    public int MessageCount { get; set; }
}

public class SourceDocumentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public int ChunkCount { get; set; }
    public string? ChapterTitle { get; set; }
    public bool IsSelected { get; set; } = true;
}

public class ChatSubjectDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
}

public class ChatWorkspaceViewModel
{
    public int CurrentSubjectId { get; set; }
    public string CurrentSubjectCode { get; set; } = string.Empty;
    public string CurrentSubjectName { get; set; } = string.Empty;
    public List<ChatSubjectDto> AvailableSubjects { get; set; } = [];
    public List<SourceDocumentDto> SourceDocuments { get; set; } = [];
    public List<ChatSessionDto> RecentSessions { get; set; } = [];
    public ChatSessionDto? ActiveSession { get; set; }
    public List<ChatMessageDto> ActiveMessages { get; set; } = [];
}

public class SendChatRequest
{
    public int? SessionId { get; set; }
    public int SubjectId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<int> SelectedDocumentIds { get; set; } = [];
}

public class SendChatResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public ChatMessageDto? UserMessage { get; set; }
    public ChatMessageDto? AssistantMessage { get; set; }
}

public class ChatSessionMessagesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<int> SelectedDocumentIds { get; set; } = [];
    public List<ChatMessageDto> Messages { get; set; } = [];
}

public class ChatStreamPacket
{
    public string Type { get; set; } = "token"; // "init", "token", "done", "error"
    public string Token { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public ChatMessageDto? UserMessage { get; set; }
    public ChatMessageDto? AssistantMessage { get; set; }
}
