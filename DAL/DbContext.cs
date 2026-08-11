using BLL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        // Constructor accepting options is required for Dependency Injection (DI)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Expose your database tables as DbSets
        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SystemAccount> SystemAccounts { get; set; }
        public DbSet<Tag> Tags { get; set; }

        // Add this fallback constructor for Design-Time tooling
        public AppDbContext()
        {
        }
    }
}
