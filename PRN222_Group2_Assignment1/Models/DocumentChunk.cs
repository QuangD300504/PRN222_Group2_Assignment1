using System.ComponentModel.DataAnnotations;

namespace PRN222_Group2_Assignment1.Models;

public class DocumentChunk
{
    public int Id { get; set; }

    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public int ChunkIndex { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    public int PageNumber { get; set; } = 1;

    [MaxLength(200)]
    public string? Heading { get; set; }

    public int TokenCount { get; set; }

    public bool HasEmbedding { get; set; }

    public string? EmbeddingVectorJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
