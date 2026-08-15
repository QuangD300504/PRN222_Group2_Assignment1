using Microsoft.EntityFrameworkCore;
using PRN222_Group2_Assignment1.Models;

namespace PRN222_Group2_Assignment1.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Subject>(e =>
        {
            e.HasIndex(s => s.Code).IsUnique();
        });

        modelBuilder.Entity<Chapter>(e =>
        {
            e.HasOne(c => c.Subject)
             .WithMany(s => s.Chapters)
             .HasForeignKey(c => c.SubjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Document>(e =>
        {
            e.HasOne(d => d.Subject)
             .WithMany(s => s.Documents)
             .HasForeignKey(d => d.SubjectId)
             .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(d => d.Chapter)
             .WithMany(c => c.Documents)
             .HasForeignKey(d => d.ChapterId)
             .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(d => d.UploadedBy)
             .WithMany()
             .HasForeignKey(d => d.UploadedById)
             .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DocumentChunk>(e =>
        {
            e.HasOne(c => c.Document)
             .WithMany(d => d.Chunks)
             .HasForeignKey(c => c.DocumentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatSession>(e =>
        {
            e.HasOne(s => s.Subject)
             .WithMany()
             .HasForeignKey(s => s.SubjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.User)
             .WithMany()
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(e =>
        {
            e.HasOne(m => m.Session)
             .WithMany(s => s.Messages)
             .HasForeignKey(m => m.SessionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Seed Data ─────────────────────────────────────────────────────────────────
        var seedUser = new AppUser
        {
            Id = 1,
            Email = "leader@chatbot.edu.vn",
            FullName = "Subject Leader",
            Password = "leader@123",
            Role = "SubjectLeader",
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var seedStudent = new AppUser
        {
            Id = 2,
            Email = "student@gmail.com",
            FullName = "Nguyen Van A (Student)",
            Password = "student@123",
            Role = "Student",
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var seedSubject = new Subject
        {
            Id = 1,
            Code = "PRN222",
            Name = "Enterprise Web Application Development",
            Description = "Advanced C# .NET Core MVC, Entity Framework Core, Real-time Web Apps, and AI Integrations.",
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var ch1 = new Chapter { Id = 1, SubjectId = 1, ChapterNumber = 1, Title = "Chapter 1: Entity Framework Core & Data Architecture", Summary = "DbContext, Migration, Lazy Loading, Indexing, and Repository Pattern", CreatedAt = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc) };
        var ch2 = new Chapter { Id = 2, SubjectId = 1, ChapterNumber = 2, Title = "Chapter 2: Async & Parallel Programming in C#", Summary = "Tasks, Async/Await, Task.WhenAll, Thread Pooling, and Concurrency Controls", CreatedAt = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc) };
        var ch3 = new Chapter { Id = 3, SubjectId = 1, ChapterNumber = 3, Title = "Chapter 3: AI Document Processing & RAG Integration", Summary = "Text Extraction, Chunking Strategies, Vector Embeddings, and Retrieval Augmented Generation", CreatedAt = new DateTime(2025, 1, 4, 0, 0, 0, DateTimeKind.Utc) };

        var doc1 = new Document
        {
            Id = 1,
            Title = "EF Core 8 High Performance & Best Practices Guide",
            FileName = "EFCore_BestPractices_PRN222.pdf",
            FileExtension = "pdf",
            MimeType = "application/pdf",
            FileSizeBytes = 2458000,
            ContentHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
            StoragePath = "uploads/prn222/EFCore_BestPractices_PRN222.pdf",
            SubjectId = 1,
            ChapterId = 1,
            UploadedById = 1,
            Status = "Ready",
            ChunkCount = 4,
            UploadedAt = new DateTime(2025, 1, 10, 8, 30, 0, DateTimeKind.Utc),
            IndexedAt = new DateTime(2025, 1, 10, 8, 32, 0, DateTimeKind.Utc)
        };

        var doc2 = new Document
        {
            Id = 2,
            Title = "C# Async Await Deep Dive Lecture Slides",
            FileName = "Async_Await_Mastery.pptx",
            FileExtension = "pptx",
            MimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            FileSizeBytes = 5120000,
            ContentHash = "8f434346648f6b96df89dda901c5176b10a6d83961dd3c1ac88b59b2dc327aa4",
            StoragePath = "uploads/prn222/Async_Await_Mastery.pptx",
            SubjectId = 1,
            ChapterId = 2,
            UploadedById = 1,
            Status = "Ready",
            ChunkCount = 3,
            UploadedAt = new DateTime(2025, 1, 12, 14, 15, 0, DateTimeKind.Utc),
            IndexedAt = new DateTime(2025, 1, 12, 14, 16, 30, DateTimeKind.Utc)
        };

        var doc3 = new Document
        {
            Id = 3,
            Title = "RAG Architecture & Semantic Chunking Specification",
            FileName = "RAG_Chunking_Spec.docx",
            FileExtension = "docx",
            MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileSizeBytes = 1280000,
            ContentHash = "a4567223bfe70911228834900a01299dfa89110298a0011bbcd001299ff01234",
            StoragePath = "uploads/prn222/RAG_Chunking_Spec.docx",
            SubjectId = 1,
            ChapterId = 3,
            UploadedById = 1,
            Status = "Ready",
            ChunkCount = 3,
            UploadedAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            IndexedAt = new DateTime(2025, 1, 15, 10, 02, 10, DateTimeKind.Utc)
        };

        modelBuilder.Entity<AppUser>().HasData(seedUser);
        modelBuilder.Entity<Subject>().HasData(seedSubject);
        modelBuilder.Entity<Chapter>().HasData(ch1, ch2, ch3);
        modelBuilder.Entity<Document>().HasData(doc1, doc2, doc3);

        modelBuilder.Entity<DocumentChunk>().HasData(
            new DocumentChunk { Id = 1, DocumentId = 1, ChunkIndex = 1, PageNumber = 1, Heading = "1. DbContext Lifecycle", Content = "DbContext in Entity Framework Core is designed to be short-lived. Always use AddDbContext with Scoped lifetime in ASP.NET Core web applications.", TokenCount = 120, HasEmbedding = true },
            new DocumentChunk { Id = 2, DocumentId = 1, ChunkIndex = 2, PageNumber = 3, Heading = "2. AsNoTracking Query Optimization", Content = "For read-only queries, calling AsNoTracking() avoids change tracker overhead, boosting performance by up to 30% for large data retrieval.", TokenCount = 145, HasEmbedding = true },
            new DocumentChunk { Id = 3, DocumentId = 1, ChunkIndex = 3, PageNumber = 5, Heading = "3. Explicit Indexing on Foreign Keys", Content = "Ensure foreign keys and frequently filtered columns are indexed in EF Core using HasIndex() in OnModelCreating.", TokenCount = 110, HasEmbedding = true },
            new DocumentChunk { Id = 4, DocumentId = 1, ChunkIndex = 4, PageNumber = 8, Heading = "4. Compiled Queries", Content = "Use EF.CompileAsyncQuery for hot paths to bypass query expression tree parsing overhead.", TokenCount = 95, HasEmbedding = true },

            new DocumentChunk { Id = 5, DocumentId = 2, ChunkIndex = 1, PageNumber = 2, Heading = "Slide 2: Thread Pool & Synchronization Context", Content = "Async methods yield control back to the calling thread when awaiting an incomplete Task, preventing UI or HTTP worker thread blocking.", TokenCount = 130, HasEmbedding = true },
            new DocumentChunk { Id = 6, DocumentId = 2, ChunkIndex = 2, PageNumber = 5, Heading = "Slide 5: Avoid Async Void", Content = "Async void should only be used for event handlers. Returning Task allows exception propagation and proper async orchestration.", TokenCount = 105, HasEmbedding = true },
            new DocumentChunk { Id = 7, DocumentId = 2, ChunkIndex = 3, PageNumber = 9, Heading = "Slide 9: Parallel Processing with Task.WhenAll", Content = "Execute independent HTTP or I/O bound requests concurrently using Task.WhenAll to achieve high throughput.", TokenCount = 125, HasEmbedding = true },

            new DocumentChunk { Id = 8, DocumentId = 3, ChunkIndex = 1, PageNumber = 1, Heading = "Section 1: Semantic Chunking Strategy", Content = "Semantic chunking splits text at logical structural boundaries such as headings, paragraphs, and lists, preserving original context.", TokenCount = 160, HasEmbedding = true },
            new DocumentChunk { Id = 9, DocumentId = 3, ChunkIndex = 2, PageNumber = 2, Heading = "Section 2: Sliding Window & Overlap", Content = "Including a 100-token sliding window overlap between adjacent chunks prevents information loss at chunk boundaries during retrieval.", TokenCount = 150, HasEmbedding = true },
            new DocumentChunk { Id = 10, DocumentId = 3, ChunkIndex = 3, PageNumber = 4, Heading = "Section 3: Embedding Vector Storage", Content = "Generated embeddings are stored alongside chunk index and metadata, allowing fast cosine similarity search in the RAG retrieval pipeline.", TokenCount = 140, HasEmbedding = true }
        );
    }
}
