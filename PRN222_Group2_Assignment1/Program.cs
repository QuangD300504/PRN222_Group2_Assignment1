using Microsoft.EntityFrameworkCore;
using PRN222_Group2_Assignment1.Data;
using PRN222_Group2_Assignment1.Services;

var builder = WebApplication.CreateBuilder(args);

// Load local overrides (git-ignored) — each dev sets their own connection string here
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// ── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IRagChatService, RagChatService>();
builder.Services.AddHttpClient();

// ── Session (used for login state) ───────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── MVC ──────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Auto Migrate & Seed DB ──────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    // Seed default Subject Leader if missing
    if (!dbContext.AppUsers.Any(u => u.Email == "leader@gmail.com" || u.Role == "SubjectLeader"))
    {
        dbContext.AppUsers.Add(new PRN222_Group2_Assignment1.Models.AppUser
        {
            Email = "leader@gmail.com",
            FullName = "Subject Leader",
            Password = "leader@123",
            Role = "SubjectLeader",
            CreatedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();
    }

    // Seed default Student if missing
    if (!dbContext.AppUsers.Any(u => u.Email == "student@gmail.com"))
    {
        dbContext.AppUsers.Add(new PRN222_Group2_Assignment1.Models.AppUser
        {
            Email = "student@gmail.com",
            FullName = "Student",
            Password = "student@123",
            Role = "Student",
            CreatedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
