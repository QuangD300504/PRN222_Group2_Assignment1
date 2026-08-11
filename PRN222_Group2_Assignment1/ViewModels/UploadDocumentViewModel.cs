namespace PRN222_Group2_Assignment1.ViewModels;

public class UploadDocumentViewModel
{
    public IFormFile File { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int SubjectId { get; set; }
    public int? ChapterId { get; set; }

    // Inline chapter creation
    public string? NewChapterTitle { get; set; }
    public int? NewChapterNumber { get; set; }
}
