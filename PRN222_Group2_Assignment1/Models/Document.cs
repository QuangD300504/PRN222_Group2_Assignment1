using System.ComponentModel.DataAnnotations;

namespace PRN222_Group2_Assignment1.Models;

public class Document
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(255)]
    public string FileName { get; set; } = null!;

    [Required, MaxLength(50)]
    public string FileExtension { get; set; } = null!; // pdf, docx, pptx

    [Required, MaxLength(100)]
    public string MimeType { get; set; } = null!;

    public long FileSizeBytes { get; set; }

    [MaxLength(128)]
    public string? ContentHash { get; set; } // SHA256 deduplication

    [MaxLength(500)]
    public string? StoragePath { get; set; }

    public int? SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public int? ChapterId { get; set; }
    public Chapter? Chapter { get; set; }

    public int UploadedById { get; set; }
    public AppUser UploadedBy { get; set; } = null!;

    /// <summary>Pending | Processing | Ready | Failed</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public int ChunkCount { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? IndexedAt { get; set; }

    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}
