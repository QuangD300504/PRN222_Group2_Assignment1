using System.ComponentModel.DataAnnotations;

namespace PRN222_Group2_Assignment1.Models;

public class Chapter
{
    public int Id { get; set; }

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public int ChapterNumber { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;

    [MaxLength(500)]
    public string? Summary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
