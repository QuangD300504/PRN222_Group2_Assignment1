using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PRN222_Group2_Assignment1.Data;
using PRN222_Group2_Assignment1.Models;
using PRN222_Group2_Assignment1.ViewModels;

namespace PRN222_Group2_Assignment1.Services;

public partial class RagChatService : IRagChatService
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RagChatService(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<ChatWorkspaceViewModel> GetWorkspaceDataAsync(int? subjectId, int? sessionId, int userId)
    {
        var subjects = await _context.Subjects
            .OrderBy(s => s.Code)
            .Select(s => new ChatSubjectDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                DocumentCount = s.Documents.Count(d => d.Status == "Ready")
            })
            .ToListAsync();

        var activeSubject = subjects.FirstOrDefault(s => s.Id == subjectId)
                            ?? subjects.FirstOrDefault()
                            ?? new ChatSubjectDto { Id = 1, Code = "PRN222", Name = "Enterprise Web App" };

        var sourceDocuments = await _context.Documents
            .Where(d => d.SubjectId == activeSubject.Id && d.Status == "Ready")
            .Include(d => d.Chapter)
            .OrderBy(d => d.ChapterId)
            .ThenBy(d => d.Title)
            .Select(d => new SourceDocumentDto
            {
                Id = d.Id,
                Title = d.Title,
                FileExtension = d.FileExtension,
                ChunkCount = d.ChunkCount,
                ChapterTitle = d.Chapter != null ? d.Chapter.Title : "Chung",
                IsSelected = true
            })
            .ToListAsync();

        var recentSessions = await _context.ChatSessions
            .Where(s => s.SubjectId == activeSubject.Id && s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .Select(s => new ChatSessionDto
            {
                Id = s.Id,
                Title = s.Title,
                SubjectId = s.SubjectId,
                SubjectCode = activeSubject.Code,
                SelectedDocumentIds = ParseDocIds(s.SelectedDocumentIdsJson),
                UpdatedAt = s.UpdatedAt,
                MessageCount = s.Messages.Count
            })
            .ToListAsync();

        ChatSessionDto? activeSessionDto = null;
        List<ChatMessageDto> activeMessages = [];

        if (sessionId.HasValue && sessionId.Value > 0)
        {
            var session = await _context.ChatSessions
                .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(s => s.Id == sessionId.Value && s.UserId == userId);

            if (session != null)
            {
                activeSessionDto = new ChatSessionDto
                {
                    Id = session.Id,
                    Title = session.Title,
                    SubjectId = session.SubjectId,
                    SubjectCode = activeSubject.Code,
                    SelectedDocumentIds = ParseDocIds(session.SelectedDocumentIdsJson),
                    UpdatedAt = session.UpdatedAt,
                    MessageCount = session.Messages.Count
                };

                activeMessages = session.Messages.Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    Citations = ParseCitations(m.CitationsJson),
                    CreatedAt = m.CreatedAt
                }).ToList();
            }
        }

        return new ChatWorkspaceViewModel
        {
            CurrentSubjectId = activeSubject.Id,
            CurrentSubjectCode = activeSubject.Code,
            CurrentSubjectName = activeSubject.Name,
            AvailableSubjects = subjects,
            SourceDocuments = sourceDocuments,
            RecentSessions = recentSessions,
            ActiveSession = activeSessionDto,
            ActiveMessages = activeMessages
        };
    }

    public async Task<ChatSessionDto> CreateSessionAsync(int subjectId, int userId, string? title = null)
    {
        var subject = await _context.Subjects.FindAsync(subjectId);
        var session = new ChatSession
        {
            SubjectId = subjectId,
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(title) ? "Cuộc trò chuyện mới" : title.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        return new ChatSessionDto
        {
            Id = session.Id,
            Title = session.Title,
            SubjectId = session.SubjectId,
            SubjectCode = subject?.Code ?? "PRN222",
            SelectedDocumentIds = [],
            UpdatedAt = session.UpdatedAt,
            MessageCount = 0
        };
    }

    public async Task<ChatSessionMessagesResponse> GetSessionMessagesAsync(int sessionId, int userId)
    {
        var session = await _context.ChatSessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null)
        {
            return new ChatSessionMessagesResponse
            {
                Success = false,
                Message = "Không tìm thấy cuộc trò chuyện."
            };
        }

        var messages = session.Messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            Role = m.Role,
            Content = m.Content,
            Citations = ParseCitations(m.CitationsJson),
            CreatedAt = m.CreatedAt
        }).ToList();

        return new ChatSessionMessagesResponse
        {
            Success = true,
            SessionId = session.Id,
            Title = session.Title,
            SelectedDocumentIds = ParseDocIds(session.SelectedDocumentIdsJson),
            Messages = messages
        };
    }

    public async Task<bool> DeleteSessionAsync(int sessionId, int userId)
    {
        var session = await _context.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session == null) return false;

        _context.ChatSessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RenameSessionAsync(int sessionId, int userId, string newTitle)
    {
        var session = await _context.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session == null || string.IsNullOrWhiteSpace(newTitle)) return false;

        session.Title = newTitle.Trim();
        session.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CitationDto>> RetrieveRelevantChunksAsync(string query, List<int> selectedDocIds, int topK = 4)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        IQueryable<DocumentChunk> chunksQuery = _context.DocumentChunks.Include(c => c.Document);

        if (selectedDocIds != null && selectedDocIds.Count > 0)
        {
            chunksQuery = chunksQuery.Where(c => selectedDocIds.Contains(c.DocumentId));
        }

        var candidateChunks = await chunksQuery
            .Select(c => new
            {
                c.Id,
                c.DocumentId,
                DocTitle = c.Document.Title,
                c.PageNumber,
                c.Heading,
                c.Content
            })
            .ToListAsync();

        if (candidateChunks.Count == 0) return [];

        // Vector / Lexical BM25 Cosine Scoring
        var queryTokens = Tokenize(query);
        if (queryTokens.Count == 0) return [];

        var scored = candidateChunks.Select(chunk =>
        {
            var chunkTokens = Tokenize(chunk.Content + " " + (chunk.Heading ?? ""));
            double score = ComputeSimilarity(queryTokens, chunkTokens);
            return new
            {
                Chunk = chunk,
                Score = score
            };
        })
        .Where(x => x.Score > 0.05)
        .OrderByDescending(x => x.Score)
        .Take(topK)
        .ToList();

        var citations = new List<CitationDto>();
        for (int i = 0; i < scored.Count; i++)
        {
            var item = scored[i];
            citations.Add(new CitationDto
            {
                Index = i + 1,
                ChunkId = item.Chunk.Id,
                DocumentTitle = item.Chunk.DocTitle,
                PageNumber = item.Chunk.PageNumber,
                Heading = item.Chunk.Heading,
                Snippet = TruncateSnippet(item.Chunk.Content, 280),
                SimilarityScore = Math.Round(item.Score, 3)
            });
        }

        return citations;
    }

    public async Task<SendChatResponse> ProcessChatQueryAsync(SendChatRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new SendChatResponse { Success = false, Message = "Nội dung câu hỏi không được để trống." };
        }

        var subject = await _context.Subjects.FindAsync(request.SubjectId);
        var subjectCode = subject?.Code ?? "PRN222";

        ChatSession? session = null;
        if (request.SessionId.HasValue && request.SessionId.Value > 0)
        {
            session = await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId.Value && s.UserId == userId);
        }

        if (session == null)
        {
            var initialTitle = request.Message.Length > 36 ? request.Message[..36] + "..." : request.Message;
            session = new ChatSession
            {
                SubjectId = request.SubjectId,
                UserId = userId,
                Title = initialTitle,
                SelectedDocumentIdsJson = request.SelectedDocumentIds != null ? JsonSerializer.Serialize(request.SelectedDocumentIds) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();
        }
        else
        {
            session.UpdatedAt = DateTime.UtcNow;
            if (request.SelectedDocumentIds != null)
            {
                session.SelectedDocumentIdsJson = JsonSerializer.Serialize(request.SelectedDocumentIds);
            }
        }

        // 1. Save User Message
        var userMessage = new ChatMessage
        {
            SessionId = session.Id,
            Role = "user",
            Content = request.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _context.ChatMessages.Add(userMessage);
        await _context.SaveChangesAsync();

        // 2. RAG Retrieval Step
        var citations = await RetrieveRelevantChunksAsync(request.Message, request.SelectedDocumentIds ?? [], topK: 4);

        // 3. Grounding & Response Synthesis (with Local Qwen2.5:7b LLM via Ollama + fallback)
        string assistantAnswer;
        List<string> followUps = [];

        if (citations.Count == 0)
        {
            // Strict Anti-Hallucination Guardrail Refusal
            assistantAnswer = "⚠️ **Thông báo từ Hệ thống RAG**:\n\n" +
                              "Tài liệu môn học trong phạm vi được chọn **không chứa thông tin liên quan** đến câu hỏi của bạn.\n\n" +
                              "*Theo nguyên tắc giới hạn phạm vi tài liệu (Strict Grounding), AI không sử dụng kiến thức bên ngoài để phỏng đoán. Vui lòng kiểm tra lại danh sách tài liệu được chọn ở cột bên trái hoặc tham khảo bài giảng chính thức.*";
            followUps = [
                "Tóm tắt các tài liệu đang có trong môn học này?",
                "Những chủ đề nào được đề cập trong bài giảng?",
                "Hướng dẫn cách chọn tài liệu để hỏi AI?"
            ];
        }
        else
        {
            (assistantAnswer, followUps) = await GenerateAnswerWithLlmAsync(request.Message, citations, subjectCode);
        }

        // 4. Save Assistant Message
        var assistantMessage = new ChatMessage
        {
            SessionId = session.Id,
            Role = "assistant",
            Content = assistantAnswer,
            CitationsJson = citations.Count > 0 ? JsonSerializer.Serialize(citations) : null,
            CreatedAt = DateTime.UtcNow
        };
        _context.ChatMessages.Add(assistantMessage);
        await _context.SaveChangesAsync();

        return new SendChatResponse
        {
            Success = true,
            SessionId = session.Id,
            SessionTitle = session.Title,
            UserMessage = new ChatMessageDto
            {
                Id = userMessage.Id,
                Role = "user",
                Content = userMessage.Content,
                CreatedAt = userMessage.CreatedAt
            },
            AssistantMessage = new ChatMessageDto
            {
                Id = assistantMessage.Id,
                Role = "assistant",
                Content = assistantMessage.Content,
                Citations = citations,
                SuggestedFollowUps = followUps,
                CreatedAt = assistantMessage.CreatedAt
            }
        };
    }

    #region LLM & Helper Methods

    private async Task<(string Answer, List<string> FollowUps)> GenerateAnswerWithLlmAsync(string query, List<CitationDto> citations, string subjectCode)
    {
        var ollamaBaseUrl = _configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        var ollamaModel = _configuration["Ollama:Model"] ?? "qwen2.5:7b";

        // Build Structured Grounding Prompt with Follow-Up Request
        var promptBuilder = new System.Text.StringBuilder();
        promptBuilder.AppendLine($"BẠN LÀ TRỢ LÝ HỌC TẬP AI DÀNH RIÊNG CHO MÔN HỌC {subjectCode}.");
        promptBuilder.AppendLine("QUY TẮC BẮT BUỘC (STRICT GROUNDING):");
        promptBuilder.AppendLine("1. BẠN CHỈ ĐƯỢC TRẢ LỜI DỰA TRÊN THÔNG TIN TRONG PHẦN [NGỮ CẢNH TÀI LIỆU] DƯỚI ĐÂY.");
        promptBuilder.AppendLine("2. TUYỆT ĐỐI KHÔNG SỬ DỤNG KIẾN THỨC BÊN NGOÀI ĐỂ BỔ SUNG.");
        promptBuilder.AppendLine("3. ĐÍNH KÈM TRÍCH DẪN NGUỒN CỤ THỂ DƯỚI DẠNG [1], [2] TƯƠNG ỨNG VỚI NGUỒN ĐƯỢC ĐÁNH SỐ.");
        promptBuilder.AppendLine("4. NẾU TÀI LIỆU KHÔNG CHỨA ĐỦ THÔNG TIN, HÃY NÓI RÕ RẰNG TÀI LIỆU KHÔNG ĐỀ CẬP.");
        promptBuilder.AppendLine("5. CUỐI CÂU TRẢ LỜI, HÃY GỢI Ý 2 ĐẾN 3 CÂU HỎI TIẾP THEO THEO ĐỊNH DẠNG:");
        promptBuilder.AppendLine("---SUGGESTED_QUESTIONS---");
        promptBuilder.AppendLine("- Câu hỏi gợi ý 1?");
        promptBuilder.AppendLine("- Câu hỏi gợi ý 2?");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("[NGỮ CẢNH TÀI LIỆU]:");

        foreach (var c in citations)
        {
            promptBuilder.AppendLine($"---");
            promptBuilder.AppendLine($"[Nguồn {c.Index}] Tài liệu: {c.DocumentTitle} | Trang {c.PageNumber} | Mục: {c.Heading ?? "Chung"}");
            promptBuilder.AppendLine($"Nội dung: {c.Snippet}");
        }
        promptBuilder.AppendLine("---");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine($"CÂU HỎI CỦA SINH VIÊN: {query}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("CÂU TRẢ LỜI (Được trích dẫn chuẩn xác):");

        var prompt = promptBuilder.ToString();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(35);

            var requestBody = new
            {
                model = ollamaModel,
                prompt,
                stream = false,
                options = new
                {
                    temperature = 0.2,
                    top_p = 0.9
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{ollamaBaseUrl.TrimEnd('/')}/api/generate", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OllamaResponse>(responseString);
                if (result != null && !string.IsNullOrWhiteSpace(result.Response))
                {
                    return ParseLlmResponseAndFollowUps(result.Response.Trim(), citations);
                }
            }
        }
        catch
        {
            // Ollama is offline, fallback to deterministic semantic extraction
        }

        // Fallback Synthesizer
        return FallbackSynthesizeGroundedAnswer(query, citations);
    }

    private static (string Answer, List<string> FollowUps) ParseLlmResponseAndFollowUps(string fullText, List<CitationDto> citations)
    {
        var delimiter = "---SUGGESTED_QUESTIONS---";
        var followUps = new List<string>();

        if (fullText.Contains(delimiter, StringComparison.OrdinalIgnoreCase))
        {
            var parts = fullText.Split([delimiter], StringSplitOptions.None);
            var answer = parts[0].Trim();
            var questionsPart = parts.Length > 1 ? parts[1].Trim() : "";

            var lines = questionsPart.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var clean = line.Trim().TrimStart('-', '*', '•', '1', '2', '3', '.', ' ').Trim();
                if (!string.IsNullOrWhiteSpace(clean) && clean.Length > 5)
                {
                    followUps.Add(clean);
                }
            }

            return (answer, followUps.Count > 0 ? followUps.Take(3).ToList() : GenerateDynamicFollowUps(citations));
        }

        return (fullText, GenerateDynamicFollowUps(citations));
    }

    private static (string Answer, List<string> FollowUps) FallbackSynthesizeGroundedAnswer(string query, List<CitationDto> citations)
    {
        var primaryCitation = citations[0];
        var badgeList = string.Join(" ", citations.Select(c => $"[{c.Index}]"));

        var responseBuilder = new System.Text.StringBuilder();
        responseBuilder.AppendLine($"Dựa trên các tài liệu học tập được chọn {badgeList}, tổng hợp nội dung cho câu hỏi của bạn như sau:\n");

        for (int i = 0; i < citations.Count; i++)
        {
            var cit = citations[i];
            var cleanSnippet = cit.Snippet.Replace("\r", "").Replace("\n", " ").Trim();
            if (cleanSnippet.Length > 160) cleanSnippet = cleanSnippet[..160] + "...";

            responseBuilder.AppendLine($"• **Trọng tâm {i + 1}**: {cleanSnippet} [{cit.Index}]");
        }

        responseBuilder.AppendLine();
        responseBuilder.AppendLine($"> 📖 *Nguồn trích dẫn: **{primaryCitation.DocumentTitle}** (Trang {primaryCitation.PageNumber})*");

        var followUps = GenerateDynamicFollowUps(citations);
        return (responseBuilder.ToString(), followUps);
    }

    private static List<string> GenerateDynamicFollowUps(List<CitationDto> citations)
    {
        var list = new List<string>();
        if (citations.Count > 0)
        {
            var doc1 = citations[0];
            list.Add($"Giải thích chi tiết hơn về nội dung trong {doc1.DocumentTitle} (Trang {doc1.PageNumber})?");
        }
        if (citations.Count > 1)
        {
            var doc2 = citations[1];
            list.Add($"So sánh điểm khác biệt với phần trong {doc2.DocumentTitle}?");
        }
        list.Add("Tóm tắt lại các điểm cốt lõi cần ghi nhớ?");
        return list.Take(3).ToList();
    }

    private static List<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var words = WordRegex().Matches(text.ToLowerInvariant())
            .Select(m => m.Value)
            .Where(w => w.Length > 1)
            .Distinct()
            .ToList();
        return words;
    }

    private static double ComputeSimilarity(List<string> queryTokens, List<string> chunkTokens)
    {
        if (queryTokens.Count == 0 || chunkTokens.Count == 0) return 0.0;
        var intersection = queryTokens.Intersect(chunkTokens).Count();
        if (intersection == 0) return 0.0;

        // Jaccard-Cosine Hybrid Metric
        double score = (double)intersection / Math.Sqrt(queryTokens.Count * chunkTokens.Count);
        return score;
    }

    private static string TruncateSnippet(string content, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(content)) return "";
        var clean = WhiteSpaceRegex().Replace(content.Trim(), " ");
        return clean.Length <= maxLen ? clean : clean[..maxLen] + "...";
    }

    private static List<int> ParseDocIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<int>>(json) ?? []; }
        catch { return []; }
    }

    private static List<CitationDto> ParseCitations(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<CitationDto>>(json) ?? []; }
        catch { return []; }
    }

    [GeneratedRegex(@"[\p{L}\p{N}_]+")]
    private static partial Regex WordRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpaceRegex();

    private sealed class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }

    #endregion
}
