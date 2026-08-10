using Microsoft.EntityFrameworkCore;
using PRN222_Group2_Assignment1.Models;

namespace PRN222_Group2_Assignment1.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
        });

        // Seed a default SubjectLeader account (password: leader@123)
        modelBuilder.Entity<AppUser>().HasData(new AppUser
        {
            Id = 1,
            Email = "leader@chatbot.edu.vn",
            FullName = "Subject Leader",
            Password = "leader@123",
            Role = "SubjectLeader",
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
