using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Group2_Assignment1.Services;
using PRN222_Group2_Assignment1.ViewModels;

namespace PRN222_Group2_Assignment2.Pages.Document
{
    public class IndexModel : PageModel
    {
        private readonly IDocumentService _documentService;
        private readonly IWebHostEnvironment _env;

        public IndexModel(IDocumentService documentService, IWebHostEnvironment env)
        {
            _documentService = documentService;
            _env = env;
        }

        public DocumentManagementViewModel ViewModel { get; set; } = new DocumentManagementViewModel();

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

        public async Task<IActionResult> OnPostUploadAsync([FromForm] UploadDocumentViewModel input)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
            {
                return new JsonResult(new { success = false, message = "Chỉ có Subject Leader mới có quyền tải lên tài liệu." });
            }

            var userIdStr = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetInt32("UserId")?.ToString();
            int userId = int.TryParse(userIdStr, out var parsedId) ? parsedId : 1;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            var (success, message, document) = await _documentService.UploadDocumentAsync(input, userId, uploadsFolder);

            return new JsonResult(new
            {
                success,
                message,
                chunkCount = document?.ChunkCount,
                status = document?.Status,
                subjectId = input.SubjectId
            });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, int? subjectId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
            {
                TempData["ErrorMessage"] = "Chỉ có Subject Leader mới có quyền xóa tài liệu.";
                return RedirectToPage(new { selectedSubjectId = subjectId });
            }

            var result = await _documentService.DeleteDocumentAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Xóa tài liệu và dữ liệu chunks thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài liệu cần xóa.";
            }

            return RedirectToPage(new { selectedSubjectId = subjectId });
        }

        public async Task<IActionResult> OnPostUpdateSubjectAsync(int subjectId, string code, string name, string? description)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
                return new JsonResult(new { success = false, message = "Only Subject Leaders can edit subject details." });

            if (string.IsNullOrWhiteSpace(code))
                return new JsonResult(new { success = false, message = "Mã môn học không được để trống." });

            if (string.IsNullOrWhiteSpace(name))
                return new JsonResult(new { success = false, message = "Tên môn học không được để trống." });

            var (success, message) = await _documentService.UpdateSubjectAsync(subjectId, code, name, description);
            return new JsonResult(new { success, message });
        }

        public async Task<IActionResult> OnPostCreateSubjectAsync(string code, string name, string? description)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
            {
                return new JsonResult(new { success = false, message = "Chỉ có Subject Leader mới có quyền thêm môn học." });
            }

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                return new JsonResult(new { success = false, message = "Vui lòng nhập đầy đủ mã và tên môn học." });
            }

            var (success, message, subject) = await _documentService.CreateSubjectAsync(code, name, description);
            return new JsonResult(new { success, message, subjectId = subject?.Id });
        }

        public async Task<IActionResult> OnGetManageSubjectModalPartialAsync(int subjectId)
        {
            var viewModel = await _documentService.GetDocumentManagementDataAsync(subjectId, null, null, null);
            return Partial("_ManageSubjectModal", viewModel);
        }

        public async Task<IActionResult> OnPostDeleteSubjectAsync(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
            {
                return new JsonResult(new { success = false, message = "Chỉ có Subject Leader mới có quyền xóa môn học." });
            }

            var (success, message) = await _documentService.DeleteSubjectAsync(id);
            return new JsonResult(new { success, message });
        }

        public async Task<IActionResult> OnPostSaveChapterAsync(int subjectId, int? id, int chapterNumber, string title, string? summary)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
                return new JsonResult(new { success = false, message = "Only Subject Leaders can manage chapters." });

            if (chapterNumber <= 0 || string.IsNullOrWhiteSpace(title))
                return new JsonResult(new { success = false, message = "Vui lòng nhập số chương hợp lệ và tiêu đề chương." });

            var (success, message) = await _documentService.SaveChapterAsync(subjectId, id, chapterNumber, title, summary);
            return new JsonResult(new { success, message });
        }

        public async Task<IActionResult> OnPostDeleteChapterAsync(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
                return new JsonResult(new { success = false, message = "Only Subject Leaders can delete chapters." });

            var (success, message) = await _documentService.DeleteChapterAsync(id);
            return new JsonResult(new { success, message });
        }
    }
}
