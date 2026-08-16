using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PRN222_Group2_Assignment1.Services;
using PRN222_Group2_Assignment1.ViewModels;
using PRN222_Group2_Assignment2.Hubs;

namespace PRN222_Group2_Assignment2.Pages.Document
{
    public class IndexModel : PageModel
    {
        private readonly IDocumentService _documentService;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<DocumentHub> _hubContext;

        public IndexModel(IDocumentService documentService, IWebHostEnvironment env, IHubContext<DocumentHub> hubContext)
        {
            _documentService = documentService;
            _env = env;
            _hubContext = hubContext;
        }

        public DocumentManagementViewModel ViewModel { get; set; } = new DocumentManagementViewModel();

        private bool IsSubjectLeader()
        {
            var role = HttpContext.Session.GetString("UserRole") ?? "";
            return string.Equals(role.Trim(), "SubjectLeader", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IActionResult> OnGetAsync(int? selectedSubjectId, int? selectedChapterId, string? search, string? fileType)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Auth/Login");
            }

            ViewData["ActivePage"] = "Document";

            ViewModel = await _documentService.GetDocumentManagementDataAsync(selectedSubjectId, selectedChapterId, search, fileType);
            return Page();
        }

        public async Task<IActionResult> OnGetDocumentsPartialAsync(int? subjectId, int? chapterId, string? search, string? fileType)
        {
            var data = await _documentService.GetDocumentManagementDataAsync(subjectId, chapterId, search, fileType);
            return Partial("_DocumentTable", data);
        }

        public async Task<IActionResult> OnGetChunksAsync(int id)
        {
            var detail = await _documentService.GetDocumentChunksAsync(id);
            if (detail == null) return new JsonResult(new { message = "Document not found." });
            return new JsonResult(detail);
        }

        public async Task<IActionResult> OnGetChaptersAsync(int subjectId)
        {
            var data = await _documentService.GetDocumentManagementDataAsync(subjectId, null, null, null);
            var chapters = data.Chapters.Select(c => new { id = c.Id, title = c.Title, chapterNumber = c.ChapterNumber });
            return new JsonResult(chapters);
        }

        public async Task<IActionResult> OnPostUploadAsync([FromForm] UploadDocumentViewModel input, [FromForm] string? connectionId)
        {
            if (!IsSubjectLeader())
            {
                return new JsonResult(new { success = false, message = "Chỉ có Subject Leader mới có quyền tải lên tài liệu." });
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

            int newDocCount = 0;
            if (success && document != null)
            {
                var refreshedData = await _documentService.GetDocumentManagementDataAsync(input.SubjectId, null, null, null);
                newDocCount = refreshedData.Documents.Count;

                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("UploadProgress", 100, "4/4: Đã lưu trữ và lập chỉ mục hoàn tất!");
                }

                await _hubContext.Clients.All.SendAsync("DocumentUploaded", input.SubjectId, document.Title, newDocCount);
            }

            return new JsonResult(new
            {
                success,
                message,
                chunkCount = document?.ChunkCount,
                status = document?.Status,
                subjectId = input.SubjectId,
                newDocCount
            });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, int? subjectId)
        {
            if (!IsSubjectLeader())
            {
                TempData["ErrorMessage"] = "Chỉ có Subject Leader mới có quyền xóa tài liệu.";
                return RedirectToPage(new { selectedSubjectId = subjectId });
            }

            var result = await _documentService.DeleteDocumentAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Xóa tài liệu và dữ liệu chunks thành công!";
                var refreshedData = await _documentService.GetDocumentManagementDataAsync(subjectId, null, null, null);
                int newDocCount = refreshedData.Documents.Count;

                await _hubContext.Clients.All.SendAsync("DocumentDeleted", subjectId ?? 0, id, newDocCount);
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu cần xóa.";
            }

            return RedirectToPage(new { selectedSubjectId = subjectId });
        }

        public async Task<IActionResult> OnPostUpdateSubjectAsync(int subjectId, string code, string name, string? description)
        {
            if (!IsSubjectLeader())
                return new JsonResult(new { success = false, message = "Only Subject Leaders can edit subject details." });

            if (string.IsNullOrWhiteSpace(code))
                return new JsonResult(new { success = false, message = "Mã môn học không được để trống." });

            if (string.IsNullOrWhiteSpace(name))
                return new JsonResult(new { success = false, message = "Tên môn học không được để trống." });

            var (success, message) = await _documentService.UpdateSubjectAsync(subjectId, code, name, description);
            if (success)
            {
                await _hubContext.Clients.All.SendAsync("SubjectUpdated", subjectId, "UPDATED");
            }
            return new JsonResult(new { success, message });
        }

        public async Task<IActionResult> OnPostCreateSubjectAsync(string code, string name, string? description)
        {
            if (!IsSubjectLeader())
            {
                return new JsonResult(new { success = false, message = "Chỉ có Subject Leader mới có quyền thêm môn học." });
            }

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                return new JsonResult(new { success = false, message = "Vui lòng nhập đầy đủ mã và tên môn học." });
            }

            var (success, message, subject) = await _documentService.CreateSubjectAsync(code, name, description);
            if (success && subject != null)
            {
                await _hubContext.Clients.All.SendAsync("SubjectUpdated", subject.Id, "CREATED");
            }
            return new JsonResult(new { success, message, subjectId = subject?.Id });
        }

        public async Task<IActionResult> OnGetManageSubjectModalPartialAsync(int subjectId)
        {
            var viewModel = await _documentService.GetDocumentManagementDataAsync(subjectId, null, null, null);
            return Partial("_ManageSubjectModal", viewModel);
        }

        public async Task<IActionResult> OnPostDeleteSubjectAsync(int id)
        {
            if (!IsSubjectLeader())
            {
                return new JsonResult(new { success = false, message = "Chỉ có Subject Leader mới có quyền xóa môn học." });
            }

            var (success, message) = await _documentService.DeleteSubjectAsync(id);
            if (success)
            {
                await _hubContext.Clients.All.SendAsync("SubjectUpdated", id, "DELETED");
            }
            return new JsonResult(new { success, message });
        }

        public async Task<IActionResult> OnPostSaveChapterAsync(int subjectId, int? id, int chapterNumber, string title, string? summary)
        {
            if (!IsSubjectLeader())
                return new JsonResult(new { success = false, message = "Only Subject Leaders can manage chapters." });

            if (chapterNumber <= 0 || string.IsNullOrWhiteSpace(title))
                return new JsonResult(new { success = false, message = "Vui lòng nhập số chương hợp lệ và tiêu đề chương." });

            var (success, message) = await _documentService.SaveChapterAsync(subjectId, id, chapterNumber, title, summary);
            if (success)
            {
                await _hubContext.Clients.All.SendAsync("SubjectUpdated", subjectId, "CHAPTER_SAVED");
            }
            return new JsonResult(new { success, message });
        }

        public async Task<IActionResult> OnPostDeleteChapterAsync(int id)
        {
            if (!IsSubjectLeader())
                return new JsonResult(new { success = false, message = "Only Subject Leaders can delete chapters." });

            var (success, message) = await _documentService.DeleteChapterAsync(id);
            if (success)
            {
                await _hubContext.Clients.All.SendAsync("SubjectUpdated", 0, "CHAPTER_DELETED");
            }
            return new JsonResult(new { success, message });
        }
    }
}
