using PRN222_Group2_Assignment1.Models;
using PRN222_Group2_Assignment1.ViewModels;

namespace PRN222_Group2_Assignment1.Services;

public interface IDocumentService
{
    Task<DocumentManagementViewModel> GetDocumentManagementDataAsync(
        int? subjectId,
        int? chapterId,
        string? search,
        string? fileType);

    Task<DocumentDetailJsonViewModel?> GetDocumentChunksAsync(int documentId);

    Task<bool> DeleteDocumentAsync(int documentId);

    Task<(bool success, string message, Document? document)> UploadDocumentAsync(
        UploadDocumentViewModel model,
        int uploadedById,
        string uploadsRootPath);

    Task<Chapter> GetOrCreateChapterAsync(int subjectId, int chapterNumber, string title);

    Task<(bool success, string message)> UpdateSubjectAsync(int subjectId, string code, string name, string? description);

    Task<(bool success, string message, Subject? subject)> CreateSubjectAsync(string code, string name, string? description);

    Task<(bool success, string message)> DeleteSubjectAsync(int subjectId);

    Task<(bool success, string message)> SaveChapterAsync(int subjectId, int? chapterId, int chapterNumber, string title, string? summary);

    Task<(bool success, string message)> DeleteChapterAsync(int chapterId);
}
