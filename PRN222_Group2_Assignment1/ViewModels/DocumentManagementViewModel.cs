using PRN222_Group2_Assignment1.Models;

namespace PRN222_Group2_Assignment1.ViewModels;

public class DocumentManagementViewModel
{
    public List<Document> Documents { get; set; } = new();
    public List<Subject> Subjects { get; set; } = new();
    public List<Chapter> Chapters { get; set; } = new();

    public int? SelectedSubjectId { get; set; }
    public int? SelectedChapterId { get; set; }
    public string? SearchQuery { get; set; }
    public string? FileTypeFilter { get; set; }

    // Summary Statistics
    public int TotalDocuments { get; set; }
    public int TotalChunks { get; set; }
    public long TotalStorageBytes { get; set; }
    public int ReadyDocumentsCount { get; set; }
    public int ProcessingDocumentsCount { get; set; }

    public string FormattedTotalStorage =>
        TotalStorageBytes > 1024 * 1024 * 1024
            ? $"{TotalStorageBytes / (1024.0 * 1024.0 * 1024.0):F2} GB"
            : $"{TotalStorageBytes / (1024.0 * 1024.0):F2} MB";
}
