using Microsoft.AspNetCore.Mvc;
using PRN222_Group2_Assignment1.Services;
using PRN222_Group2_Assignment1.ViewModels;

namespace PRN222_Group2_Assignment1.Controllers;

public class DocumentController(IDocumentService documentService, IWebHostEnvironment env) : Controller
{
    [HttpGet]
    [Route("Document")]
    [Route("Document/Index")]
    public async Task<IActionResult> Index(int? subjectId, int? chapterId, string? search, string? fileType)
    {
        var viewModel = await documentService.GetDocumentManagementDataAsync(subjectId, chapterId, search, fileType);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetChunks(int id)
    {
        var result = await documentService.GetDocumentChunksAsync(id);
        if (result is null) return NotFound(new { message = "Document not found." });
        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadDocumentViewModel model)
    {
        if (HttpContext.Session.GetString("UserRole") != "SubjectLeader")
            return Json(new { success = false, message = "Only Subject Leaders can upload documents." });

        var uploadedById = HttpContext.Session.GetInt32("UserId");
        if (uploadedById is null)
            return Json(new { success = false, message = "Session expired. Please log in again." });

        var uploadsRoot = Path.Combine(env.WebRootPath, "uploads");
        var (success, message, document) = await documentService.UploadDocumentAsync(model, uploadedById.Value, uploadsRoot);

        return Json(new
        {
            success,
            message,
            chunkCount = document?.ChunkCount,
            status = document?.Status,
            subjectId = model.SubjectId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? subjectId)
    {
        if (HttpContext.Session.GetString("UserRole") != "SubjectLeader")
        {
            TempData["ErrorMessage"] = "Only Subject Leaders can delete documents.";
            return RedirectToAction(nameof(Index), new { subjectId });
        }

        await documentService.DeleteDocumentAsync(id);
        TempData["SuccessMessage"] = "Document removed successfully.";
        return RedirectToAction(nameof(Index), new { subjectId });
    }

    // Called by the upload modal to load chapters for a given subject (AJAX)
    [HttpGet]
    public async Task<IActionResult> GetChapters(int subjectId)
    {
        var data = await documentService.GetDocumentManagementDataAsync(subjectId, null, null, null);
        return Json(data.Chapters.Select(c => new { c.Id, c.ChapterNumber, c.Title }));
    }

    // Called by subject tab clicks — returns only the table+filter partial (no full page reload)
    [HttpGet]
    public async Task<IActionResult> GetDocumentsPartial(int subjectId, int? chapterId, string? search, string? fileType)
    {
        var viewModel = await documentService.GetDocumentManagementDataAsync(subjectId, chapterId, search, fileType);
        return PartialView("_DocumentTable", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSubject(int subjectId, string name, string? description)
    {
        if (HttpContext.Session.GetString("UserRole") != "SubjectLeader")
            return Json(new { success = false, message = "Only Subject Leaders can edit subject details." });

        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Tên môn học không được để trống." });

        var (success, message) = await documentService.UpdateSubjectAsync(subjectId, name, description);
        return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubject(string code, string name, string? description)
    {
        if (HttpContext.Session.GetString("UserRole") != "SubjectLeader")
            return Json(new { success = false, message = "Only Subject Leaders can create new subjects." });

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Mã môn học và tên môn học không được để trống." });

        var (success, message, subject) = await documentService.CreateSubjectAsync(code, name, description);
        return Json(new { success, message, subjectId = subject?.Id });
    }

    [HttpGet]
    public async Task<IActionResult> ManageSubjectModalPartial(int subjectId)
    {
        var viewModel = await documentService.GetDocumentManagementDataAsync(subjectId, null, null, null);
        return PartialView("_ManageSubjectModal", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "SubjectLeader")
            return Json(new { success = false, message = "Only Subject Leaders can delete subjects." });

        var (success, message) = await documentService.DeleteSubjectAsync(id);
        return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveChapter(int subjectId, int? id, int chapterNumber, string title, string? summary)
    {
        if (HttpContext.Session.GetString("UserRole") != "SubjectLeader")
            return Json(new { success = false, message = "Only Subject Leaders can manage chapters." });

        if (chapterNumber <= 0 || string.IsNullOrWhiteSpace(title))
            return Json(new { success = false, message = "Vui lòng nhập số chương hợp lệ và tiêu đề chương." });

        var (success, message) = await documentService.SaveChapterAsync(subjectId, id, chapterNumber, title, summary);
        return Json(new { success, message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteChapter(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "SubjectLeader")
            return Json(new { success = false, message = "Only Subject Leaders can delete chapters." });

        var (success, message) = await documentService.DeleteChapterAsync(id);
        return Json(new { success, message });
    }
}
