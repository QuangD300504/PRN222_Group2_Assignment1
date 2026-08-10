using PRN222_Group2_Assignment1.Models;

namespace PRN222_Group2_Assignment1.Services;

public interface IAuthService
{
    /// <summary>Returns the user if credentials are valid, otherwise null.</summary>
    Task<AppUser?> LoginAsync(string email, string password);
}
