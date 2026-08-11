namespace PRN222_Group2_Assignment1.ViewModels;

public class DocumentChunkViewModel
{
    public int Id { get; set; }
    public int ChunkIndex { get; set; }
    public int PageNumber { get; set; }
    public string? Heading { get; set; }
    public string Content { get; set; } = null!;
    public int TokenCount { get; set; }
    public bool HasEmbedding { get; set; }
}

public class DocumentDetailJsonViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string FileExtension { get; set; } = null!;
    public string FormattedSize { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public string ChapterTitle { get; set; } = null!;
    public int ChunkCount { get; set; }
    public string UploadedAt { get; set; } = null!;
    public List<DocumentChunkViewModel> Chunks { get; set; } = new();
}
