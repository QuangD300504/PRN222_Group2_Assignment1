using System.ComponentModel.DataAnnotations;

namespace PRN222_Group2_Assignment1.Models;

public class ChatSession
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "Cuộc trò chuyện mới";

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    [MaxLength(500)]
    public string? SelectedDocumentIdsJson { get; set; } // e.g. "[1, 3, 5]"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
