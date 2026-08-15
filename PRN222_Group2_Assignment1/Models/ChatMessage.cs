using System.ComponentModel.DataAnnotations;

namespace PRN222_Group2_Assignment1.Models;

public class ChatMessage
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "user"; // "user" | "assistant" | "system"

    [Required]
    public string Content { get; set; } = null!;

    public string? CitationsJson { get; set; } // JSON array of cited chunk metadata

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
