using PRN222_Group2_Assignment1.ViewModels;

namespace PRN222_Group2_Assignment1.Services;

public interface IRagChatService
{
    Task<ChatWorkspaceViewModel> GetWorkspaceDataAsync(int? subjectId, int? sessionId, int userId);
    Task<ChatSessionDto> CreateSessionAsync(int subjectId, int userId, string? title = null);
    Task<ChatSessionMessagesResponse> GetSessionMessagesAsync(int sessionId, int userId);
    Task<bool> DeleteSessionAsync(int sessionId, int userId);
    Task<bool> RenameSessionAsync(int sessionId, int userId, string newTitle);
    Task<SendChatResponse> ProcessChatQueryAsync(SendChatRequest request, int userId);
    IAsyncEnumerable<ChatStreamPacket> StreamChatQueryAsync(SendChatRequest request, int userId, CancellationToken cancellationToken = default);
    Task<List<CitationDto>> RetrieveRelevantChunksAsync(string query, List<int> selectedDocIds, int topK = 4);
}
