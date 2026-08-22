using System.Net.Http.Json;
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

    public async Task<List<CitationDto>> RetrieveRelevantChunksAsync(string query, List<int> selectedDocIds, int topK = 6)
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
                c.ChunkIndex,
                DocTitle = c.Document.Title,
                c.PageNumber,
                c.Heading,
                c.Content,
                c.HasEmbedding,
                c.EmbeddingVectorJson
            })
            .ToListAsync();

        if (candidateChunks.Count == 0) return [];

        // Try to get the query vector from Ollama (nomic-embed-text)
        float[]? queryVector = await GetQueryEmbeddingAsync(query);

        var queryTokens = Tokenize(query).Where(t => t.Length > 1).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var scored = candidateChunks.Select(chunk =>
        {
            double vectorScore = 0.0;
            if (queryVector != null && chunk.HasEmbedding && !string.IsNullOrEmpty(chunk.EmbeddingVectorJson))
            {
                try
                {
                    var chunkVec = JsonSerializer.Deserialize<float[]>(chunk.EmbeddingVectorJson);
                    vectorScore = chunkVec != null ? CosineSimilarity(queryVector, chunkVec) : 0.0;
                }
                catch { vectorScore = 0.0; }
            }

            // Dynamic Lexical Overlap (Generic BM25/Sparse Token Fusion for any document)
            var chunkTokens = Tokenize(chunk.Content + " " + (chunk.Heading ?? "")).ToHashSet(StringComparer.OrdinalIgnoreCase);
            double lexicalBonus = 0.0;
            if (queryTokens.Count > 0 && chunkTokens.Count > 0)
            {
                int matchCount = queryTokens.Count(t => chunkTokens.Contains(t));
                lexicalBonus = ((double)matchCount / queryTokens.Count) * 0.15;
            }

            // Generic Hybrid Fusion: Dense Semantic Vector (nomic-embed-text) + Sparse Keyword Bonus
            double combinedScore = vectorScore > 0 
                ? (vectorScore + lexicalBonus)
                : (queryTokens.Count > 0 ? ComputeLexicalSimilarity(queryTokens.ToList(), chunkTokens.ToList()) : 0.0);

            return new { Chunk = chunk, Score = combinedScore, RawVectorScore = vectorScore };
        })
        .Where(x => x.Score > 0.15)
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
                ChunkIndex = item.Chunk.ChunkIndex + 1,
                DocumentTitle = item.Chunk.DocTitle,
                PageNumber = item.Chunk.PageNumber,
                Heading = item.Chunk.Heading,
                Snippet = TruncateSnippet(item.Chunk.Content, 2000),
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

        // 2. RAG Retrieval Step (topK = 6)
        var citations = await RetrieveRelevantChunksAsync(request.Message, request.SelectedDocumentIds ?? [], topK: 6);

        // 3. Grounding & Response Synthesis
        string assistantAnswer;

        if (citations.Count == 0)
        {
            assistantAnswer = "⚠️ **Thông báo từ Hệ thống RAG**:\n\n" +
                              "Tài liệu môn học trong phạm vi được chọn **không chứa thông tin liên quan** đến câu hỏi của bạn.\n\n" +
                              "*Theo nguyên tắc giới hạn phạm vi tài liệu (Strict Grounding), AI không sử dụng kiến thức bên ngoài để phỏng đoán. Vui lòng kiểm tra lại danh sách tài liệu được chọn ở cột bên trái hoặc tham khảo bài giảng chính thức.*";
        }
        else
        {
            assistantAnswer = await GenerateAnswerWithLlmAsync(request.Message, citations, subjectCode);
            (assistantAnswer, citations) = RenumberCitationsInOrderOfAppearance(assistantAnswer, citations);
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
                SuggestedFollowUps = [],
                CreatedAt = assistantMessage.CreatedAt
            }
        };
    }

    public async IAsyncEnumerable<ChatStreamPacket> StreamChatQueryAsync(
        SendChatRequest request, 
        int userId, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            yield return new ChatStreamPacket { Type = "error", Token = "Nội dung câu hỏi không được để trống." };
            yield break;
        }

        var subject = await _context.Subjects.FindAsync([request.SubjectId], cancellationToken);
        var subjectCode = subject?.Code ?? "PRN222";

        ChatSession? session = null;
        if (request.SessionId.HasValue && request.SessionId.Value > 0)
        {
            session = await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId.Value && s.UserId == userId, cancellationToken);
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
            await _context.SaveChangesAsync(cancellationToken);
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
        await _context.SaveChangesAsync(cancellationToken);

        var userMessageDto = new ChatMessageDto
        {
            Id = userMessage.Id,
            Role = "user",
            Content = userMessage.Content,
            CreatedAt = userMessage.CreatedAt
        };

        // Emit initialization packet to start client typing UI instantly
        yield return new ChatStreamPacket
        {
            Type = "init",
            SessionId = session.Id,
            SessionTitle = session.Title,
            UserMessage = userMessageDto
        };

        // 2. Vector Retrieval Step (topK = 6)
        var citations = await RetrieveRelevantChunksAsync(request.Message, request.SelectedDocumentIds ?? [], topK: 6);

        if (citations.Count == 0)
        {
            var guardrailText = "⚠️ **Thông báo từ Hệ thống RAG**:\n\nTài liệu môn học trong phạm vi được chọn **không chứa thông tin liên quan** đến câu hỏi của bạn.\n\n*Theo nguyên tắc giới hạn phạm vi tài liệu (Strict Grounding), AI không sử dụng kiến thức bên ngoài để phỏng đoán.*";
            yield return new ChatStreamPacket { Type = "token", Token = guardrailText, SessionId = session.Id, SessionTitle = session.Title };

            var assistantGuardMessage = new ChatMessage
            {
                SessionId = session.Id,
                Role = "assistant",
                Content = guardrailText,
                CreatedAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(assistantGuardMessage);
            await _context.SaveChangesAsync(cancellationToken);

            yield return new ChatStreamPacket
            {
                Type = "done",
                SessionId = session.Id,
                SessionTitle = session.Title,
                AssistantMessage = new ChatMessageDto
                {
                    Id = assistantGuardMessage.Id,
                    Role = "assistant",
                    Content = guardrailText,
                    CreatedAt = assistantGuardMessage.CreatedAt
                }
            };
            yield break;
        }

        // 3. Real-Time Streaming from Ollama
        var ollamaBaseUrl = _configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        var ollamaModel = _configuration["Ollama:Model"] ?? "qwen2.5:7b";

        var promptBuilder = new System.Text.StringBuilder();
        promptBuilder.AppendLine($"You are an academic study assistant for course {subjectCode}.");
        promptBuilder.AppendLine("Answer the user's question accurately, concisely, and directly based ONLY on the provided context passages below.");
        promptBuilder.AppendLine("Rules:");
        promptBuilder.AppendLine("1. Respond in the exact same language as the user's question (e.g. English if asked in English, Vietnamese if asked in Vietnamese).");
        promptBuilder.AppendLine("2. Attach inline citation markers [1], [2], etc. directly after each claim or fact extracted from that source. Always write individual markers like [1] [2], never group them like [1, 2].");
        promptBuilder.AppendLine("3. Synthesize and extract relevant facts, definitions, properties, and values from the context passages.");
        promptBuilder.AppendLine("4. If the context passages genuinely do not contain information to answer the question, state that it is not covered in the provided material.");
        promptBuilder.AppendLine("5. Do NOT output greetings, conversational filler, or introductory meta text.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("CONTEXT PASSAGES:");

        foreach (var c in citations)
        {
            promptBuilder.AppendLine($"---");
            promptBuilder.AppendLine($"[Source {c.Index}] Document: {c.DocumentTitle} | Page {c.PageNumber} | Heading: {c.Heading ?? "General"}");
            promptBuilder.AppendLine(c.Snippet);
        }
        promptBuilder.AppendLine("---");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine($"USER QUESTION: {request.Message}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("GROUNDED ANSWER:");

        var fullAnswerBuilder = new System.Text.StringBuilder();
        bool streamedSuccessfully = false;

        await foreach (var token in StreamOllamaTokensAsync(ollamaBaseUrl, ollamaModel, promptBuilder.ToString(), cancellationToken))
        {
            fullAnswerBuilder.Append(token);
            yield return new ChatStreamPacket
            {
                Type = "token",
                Token = token,
                SessionId = session.Id,
                SessionTitle = session.Title
            };
            streamedSuccessfully = true;
        }

        string finalAnswer;
        if (!streamedSuccessfully || fullAnswerBuilder.Length == 0)
        {
            finalAnswer = FallbackSynthesizeGroundedAnswer(request.Message, citations);
            yield return new ChatStreamPacket
            {
                Type = "token",
                Token = finalAnswer,
                SessionId = session.Id,
                SessionTitle = session.Title
            };
        }
        else
        {
            finalAnswer = fullAnswerBuilder.ToString().Trim();
        }

        // Renumber citations based on order of appearance in final text
        var (renumberedAnswer, finalCitations) = RenumberCitationsInOrderOfAppearance(finalAnswer, citations);

        // 4. Save Assistant Message
        var assistantMessage = new ChatMessage
        {
            SessionId = session.Id,
            Role = "assistant",
            Content = renumberedAnswer,
            CitationsJson = finalCitations.Count > 0 ? JsonSerializer.Serialize(finalCitations) : null,
            CreatedAt = DateTime.UtcNow
        };
        _context.ChatMessages.Add(assistantMessage);
        await _context.SaveChangesAsync(cancellationToken);

        yield return new ChatStreamPacket
        {
            Type = "done",
            SessionId = session.Id,
            SessionTitle = session.Title,
            AssistantMessage = new ChatMessageDto
            {
                Id = assistantMessage.Id,
                Role = "assistant",
                Content = assistantMessage.Content,
                Citations = finalCitations,
                CreatedAt = assistantMessage.CreatedAt
            }
        };
    }

    #region LLM & Helper Methods

    private async Task<string> GenerateAnswerWithLlmAsync(string query, List<CitationDto> citations, string subjectCode)
    {
        var ollamaBaseUrl = _configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        var ollamaModel = _configuration["Ollama:Model"] ?? "qwen2.5:7b";

        // Build Clean, High-Precision Grounding Prompt
        var promptBuilder = new System.Text.StringBuilder();
        promptBuilder.AppendLine($"You are an academic study assistant for course {subjectCode}.");
        promptBuilder.AppendLine("Answer the user's question accurately, concisely, and directly based ONLY on the provided context passages below.");
        promptBuilder.AppendLine("Rules:");
        promptBuilder.AppendLine("1. Respond in the exact same language as the user's question (e.g. English if asked in English, Vietnamese if asked in Vietnamese).");
        promptBuilder.AppendLine("2. Attach inline citation markers [1], [2], etc. directly after each claim or fact extracted from that source. Always write individual markers like [1] [2], never group them like [1, 2].");
        promptBuilder.AppendLine("3. Synthesize and extract relevant facts, definitions, properties, and values from the context passages.");
        promptBuilder.AppendLine("4. If the context passages genuinely do not contain information to answer the question, state that it is not covered in the provided material.");
        promptBuilder.AppendLine("5. Do NOT output greetings, conversational filler, or introductory meta text.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("CONTEXT PASSAGES:");

        foreach (var c in citations)
        {
            promptBuilder.AppendLine($"---");
            promptBuilder.AppendLine($"[Source {c.Index}] Document: {c.DocumentTitle} | Page {c.PageNumber} | Heading: {c.Heading ?? "General"}");
            promptBuilder.AppendLine(c.Snippet);
        }
        promptBuilder.AppendLine("---");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine($"USER QUESTION: {query}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("GROUNDED ANSWER:");

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
                    temperature = 0.1,
                    top_p = 0.9,
                    num_ctx = 4096,
                    num_predict = 1024
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
                    return result.Response.Trim();
                }
            }
        }
        catch
        {
            // Ollama offline / timeout
        }

        // Fallback Synthesizer
        return FallbackSynthesizeGroundedAnswer(query, citations);
    }

    private static string FallbackSynthesizeGroundedAnswer(string query, List<CitationDto> citations)
    {
        var responseBuilder = new System.Text.StringBuilder();

        for (int i = 0; i < citations.Count; i++)
        {
            var cit = citations[i];
            var cleanSnippet = cit.Snippet.Replace("\r", "").Replace("\n", " ").Trim();
            if (cleanSnippet.Length > 240) cleanSnippet = cleanSnippet[..240] + "...";

            var headingPrefix = !string.IsNullOrWhiteSpace(cit.Heading) && cit.Heading != "Chung"
                ? $"**{cit.Heading}**: "
                : "";

            responseBuilder.AppendLine($"• {headingPrefix}{cleanSnippet} [{cit.Index}]\n");
        }

        return responseBuilder.ToString().Trim();
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

    private static double ComputeLexicalSimilarity(List<string> queryTokens, List<string> chunkTokens)
    {
        if (queryTokens.Count == 0 || chunkTokens.Count == 0) return 0.0;
        var intersection = queryTokens.Intersect(chunkTokens).Count();
        if (intersection == 0) return 0.0;
        return (double)intersection / Math.Sqrt(queryTokens.Count * chunkTokens.Count);
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0.0;
        double dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++) { dot += a[i] * b[i]; magA += a[i] * a[i]; magB += b[i] * b[i]; }
        double denom = Math.Sqrt(magA) * Math.Sqrt(magB);
        return denom < 1e-8 ? 0.0 : dot / denom;
    }

    private async IAsyncEnumerable<string> StreamOllamaTokensAsync(

        string baseUrl, 
        string model, 
        string prompt, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(60);

        var requestBody = new
        {
            model,
            prompt,
            stream = true,
            options = new
            {
                temperature = 0.1,
                top_p = 0.9,
                num_ctx = 4096,
                num_predict = 1024
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/api/generate")
        {
            Content = jsonContent
        };

        HttpResponseMessage? response = null;
        try
        {
            response = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch
        {
            yield break;
        }

        if (response != null && response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                string? line = null;
                try
                {
                    line = await reader.ReadLineAsync(cancellationToken);
                }
                catch
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(line)) continue;

                OllamaResponse? chunk = null;
                try
                {
                    chunk = JsonSerializer.Deserialize<OllamaResponse>(line);
                }
                catch { }

                if (chunk != null && !string.IsNullOrEmpty(chunk.Response))
                {
                    yield return chunk.Response;
                }

                if (chunk?.Done == true) break;
            }
        }
    }

    private async Task<float[]?> GetQueryEmbeddingAsync(string query)

    {
        try
        {
            var client = _httpClientFactory.CreateClient("Ollama");
            var response = await client.PostAsJsonAsync("http://localhost:11434/api/embeddings", new
            {
                model = "nomic-embed-text",
                prompt = query
            });
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResult>();
            return result?.Embedding;
        }
        catch { return null; }
    }

    private sealed class OllamaEmbeddingResult
    {
        [JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }
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

    private static (string RenumberedAnswer, List<CitationDto> ReorderedCitations) RenumberCitationsInOrderOfAppearance(string answer, List<CitationDto> originalCitations)
    {
        if (string.IsNullOrWhiteSpace(answer) || originalCitations.Count == 0) return (answer, originalCitations);

        // Normalize grouped brackets like [1, 2] or [1, 5] into separate brackets [1] [2]
        string normalizedAnswer = Regex.Replace(answer, @"\[(\d+(?:\s*,\s*\d+)+)\]", m =>
        {
            var numbers = m.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return string.Join(" ", numbers.Select(n => $"[{n}]"));
        });

        var matches = Regex.Matches(normalizedAnswer, @"\[(\d+)\]");
        if (matches.Count == 0) return (normalizedAnswer, originalCitations);

        var oldToNewMap = new Dictionary<int, int>();
        int newIndexCounter = 1;

        foreach (Match match in matches)
        {
            if (int.TryParse(match.Groups[1].Value, out int oldIdx))
            {
                if (originalCitations.Any(c => c.Index == oldIdx) && !oldToNewMap.ContainsKey(oldIdx))
                {
                    oldToNewMap[oldIdx] = newIndexCounter++;
                }
            }
        }

        if (oldToNewMap.Count == 0) return (normalizedAnswer, originalCitations);

        // Replace in text with temp tokens first to avoid collisions
        string updatedAnswer = normalizedAnswer;
        foreach (var (oldIdx, newIdx) in oldToNewMap)
        {
            updatedAnswer = updatedAnswer.Replace($"[{oldIdx}]", $"{{#CIT_{newIdx}#}}");
        }
        foreach (var (_, newIdx) in oldToNewMap)
        {
            updatedAnswer = updatedAnswer.Replace($"{{#CIT_{newIdx}#}}", $"[{newIdx}]");
        }

        // Build reordered & renumbered list of citations
        var reorderedCitations = new List<CitationDto>();
        foreach (var (oldIdx, newIdx) in oldToNewMap.OrderBy(kv => kv.Value))
        {
            var original = originalCitations.FirstOrDefault(c => c.Index == oldIdx);
            if (original != null)
            {
                reorderedCitations.Add(new CitationDto
                {
                    Index = newIdx,
                    ChunkId = original.ChunkId,
                    ChunkIndex = original.ChunkIndex,
                    DocumentTitle = original.DocumentTitle,
                    PageNumber = original.PageNumber,
                    Heading = original.Heading,
                    Snippet = original.Snippet,
                    SimilarityScore = original.SimilarityScore
                });
            }
        }

        // Add any remaining unused citations at the end if needed
        foreach (var unused in originalCitations.Where(c => !oldToNewMap.ContainsKey(c.Index)))
        {
            reorderedCitations.Add(new CitationDto
            {
                Index = newIndexCounter++,
                ChunkId = unused.ChunkId,
                ChunkIndex = unused.ChunkIndex,
                DocumentTitle = unused.DocumentTitle,
                PageNumber = unused.PageNumber,
                Heading = unused.Heading,
                Snippet = unused.Snippet,
                SimilarityScore = unused.SimilarityScore
            });
        }

        return (updatedAnswer, reorderedCitations);
    }

    private sealed class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }

    #endregion
}

