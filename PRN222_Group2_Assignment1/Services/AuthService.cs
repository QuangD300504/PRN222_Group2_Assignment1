using Microsoft.EntityFrameworkCore;
using PRN222_Group2_Assignment1.Data;
using PRN222_Group2_Assignment1.Models;

namespace PRN222_Group2_Assignment1.Services;

public class AuthService(AppDbContext db) : IAuthService
{
    public async Task<AppUser?> LoginAsync(string email, string password) =>
        await db.AppUsers.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
}
