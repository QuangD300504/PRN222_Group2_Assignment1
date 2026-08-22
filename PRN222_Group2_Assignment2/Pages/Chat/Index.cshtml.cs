using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PRN222_Group2_Assignment1.Services;
using PRN222_Group2_Assignment1.ViewModels;
using PRN222_Group2_Assignment2.Hubs;

namespace PRN222_Group2_Assignment2.Pages.Chat;

[IgnoreAntiforgeryToken(Order = 1001)]
public class IndexModel : PageModel
{
    private readonly IRagChatService _ragChatService;
    private readonly IDocumentService _documentService;
    private readonly IWebHostEnvironment _env;
    private readonly IHubContext<DocumentHub> _hubContext;

    public IndexModel(
        IRagChatService ragChatService,
        IDocumentService documentService,
        IWebHostEnvironment env,
        IHubContext<DocumentHub> hubContext)
    {
        _ragChatService = ragChatService;
        _documentService = documentService;
        _env = env;
        _hubContext = hubContext;
    }

    public ChatWorkspaceViewModel Workspace { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? subjectId, int? sessionId)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToPage("/Auth/Login");
        }

        var userIdStr = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetInt32("UserId")?.ToString();
        int userId = int.TryParse(userIdStr, out var parsedId) ? parsedId : 1;

        Workspace = await _ragChatService.GetWorkspaceDataAsync(subjectId, sessionId, userId);
        return Page();
    }

    public async Task<IActionResult> OnPostSendMessageAsync([FromBody] SendChatRequest request)
    {
        var userIdStr = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetInt32("UserId")?.ToString();
        int userId = int.TryParse(userIdStr, out var parsedId) ? parsedId : 1;

        var response = await _ragChatService.ProcessChatQueryAsync(request, userId);
        return new JsonResult(response);
    }

    public async Task<IActionResult> OnGetSessionMessagesAsync(int sessionId)
    {
        var userIdStr = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetInt32("UserId")?.ToString();
        int userId = int.TryParse(userIdStr, out var parsedId) ? parsedId : 1;

        var response = await _ragChatService.GetSessionMessagesAsync(sessionId, userId);
        return new JsonResult(response);
    }

    public async Task<IActionResult> OnPostDeleteSessionAjaxAsync(int sessionId)
    {
        var userIdStr = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetInt32("UserId")?.ToString();
        int userId = int.TryParse(userIdStr, out var parsedId) ? parsedId : 1;

        var success = await _ragChatService.DeleteSessionAsync(sessionId, userId);
        return new JsonResult(new { success });
    }

    public async Task<IActionResult> OnGetChaptersAsync(int subjectId)
    {
        var data = await _documentService.GetDocumentManagementDataAsync(subjectId, null, null, null);
        var chapters = data.Chapters.Select(c => new { id = c.Id, title = c.Title, chapterNumber = c.ChapterNumber });
        return new JsonResult(chapters);
    }

    public async Task<IActionResult> OnPostUploadSourceAsync([FromForm] UploadDocumentViewModel input, [FromForm] string? connectionId)
    {
        var role = HttpContext.Session.GetString("UserRole") ?? "";
        if (!string.Equals(role.Trim(), "SubjectLeader", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonResult(new { success = false, message = "Chỉ Trưởng bộ môn (SubjectLeader) mới có quyền tải lên tài liệu." });
        }

        if (!string.IsNullOrEmpty(connectionId))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("UploadProgress", 20, "1/4: Đang đọc tệp và tính toán mã băm SHA-256...");
        }

        var userIdStr = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetInt32("UserId")?.ToString();
        int userId = int.TryParse(userIdStr, out var parsedId) ? parsedId : 1;

        if (!string.IsNullOrEmpty(connectionId))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("UploadProgress", 50, "2/4: Đang trích xuất nội dung trang và nhận dạng OCR...");
        }

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        var (success, message, document) = await _documentService.UploadDocumentAsync(input, userId, uploadsFolder);

        if (!string.IsNullOrEmpty(connectionId))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("UploadProgress", 85, "3/4: Đang phân mảnh văn bản với thuật toán Sliding Window...");
        }

        if (success && document != null)
        {
            var refreshedData = await _documentService.GetDocumentManagementDataAsync(input.SubjectId, null, null, null);
            var newDocCount = refreshedData.Documents.Count;

            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("UploadProgress", 100, "4/4: Hoàn tất phân mảnh văn bản!");
            }

            // Real-time broadcast to all clients (updating Documents table and other chat sessions)
            await _hubContext.Clients.All.SendAsync("DocumentUploaded", input.SubjectId, document.Title, newDocCount);

            return new JsonResult(new
            {
                success = true,
                message = message,
                document = new
                {
                    id = document.Id,
                    title = document.Title,
                    fileExtension = document.FileExtension,
                    chunkCount = document.ChunkCount,
                    chapterTitle = document.Chapter?.Title ?? "Chung"
                }
            });
        }

        return new JsonResult(new { success = false, message = message });
    }

    public async Task<IActionResult> OnPostRenameSessionAsync(int sessionId, string newTitle)
    {
        var userIdStr = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetInt32("UserId")?.ToString();
        int userId = int.TryParse(userIdStr, out var parsedId) ? parsedId : 1;

        var success = await _ragChatService.RenameSessionAsync(sessionId, userId, newTitle);
        return new JsonResult(new { success });
    }
}
