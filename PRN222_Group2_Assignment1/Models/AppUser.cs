using System.ComponentModel.DataAnnotations;

namespace PRN222_Group2_Assignment1.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Email { get; set; } = null!;

    [Required, MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Required, MaxLength(256)]
    public string Password { get; set; } = null!;

    /// <summary>SubjectLeader | Student</summary>
    [Required, MaxLength(20)]
    public string Role { get; set; } = "Student";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
