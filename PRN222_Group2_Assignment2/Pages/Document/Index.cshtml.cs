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

        public async Task<IActionResult> OnGetDocumentsPartialAsync(int? selectedSubjectId, int? selectedChapterId, string? search, string? fileType)
        {
            var data = await _documentService.GetDocumentManagementDataAsync(selectedSubjectId, selectedChapterId, search, fileType);
            return Partial("_DocumentTable", data);
        }

        public async Task<IActionResult> OnGetChunksAsync(int id)
        {
            var detail = await _documentService.GetDocumentChunksAsync(id);
            if (detail == null) return new JsonResult(new List<object>());
            return new JsonResult(detail.Chunks);
        }

        public async Task<IActionResult> OnGetChaptersAsync(int subjectId)
        {
            var data = await _documentService.GetDocumentManagementDataAsync(subjectId, null, null, null);
            var chapters = data.Chapters.Select(c => new { id = c.Id, title = c.Title, chapterNumber = c.ChapterNumber });
            return new JsonResult(chapters);
        }

        public async Task<IActionResult> OnPostUploadAsync(UploadDocumentViewModel input)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
            {
                TempData["ErrorMessage"] = "Chỉ có Subject Leader mới có quyền tải lên tài liệu.";
                return RedirectToPage();
            }

            if (input.File == null || input.File.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một tệp tài liệu để tải lên.";
                return RedirectToPage();
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            int userId = int.TryParse(userIdStr, out var parsedId) ? parsedId : 1;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            var (success, message, _) = await _documentService.UploadDocumentAsync(input, userId, uploadsFolder);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
            {
                TempData["ErrorMessage"] = "Chỉ có Subject Leader mới có quyền xóa tài liệu.";
                return RedirectToPage();
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

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateSubjectAsync(string code, string name, string? description)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
            {
                TempData["ErrorMessage"] = "Chỉ có Subject Leader mới có quyền thêm môn học.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ mã và tên môn học.";
                return RedirectToPage();
            }

            var (success, message, _) = await _documentService.CreateSubjectAsync(code, name, description);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteSubjectAsync(int subjectId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "SubjectLeader")
            {
                TempData["ErrorMessage"] = "Chỉ có Subject Leader mới có quyền xóa môn học.";
                return RedirectToPage();
            }

            var (success, message) = await _documentService.DeleteSubjectAsync(subjectId);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToPage();
        }
    }
}
