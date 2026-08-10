using System.ComponentModel.DataAnnotations;

namespace PRN222_Group2_Assignment1.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
