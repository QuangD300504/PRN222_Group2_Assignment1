using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PRN222_Group2_Assignment1.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentManagementFlow1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    ChapterNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chapters_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubjectId = table.Column<int>(type: "int", nullable: true),
                    ChapterId = table.Column<int>(type: "int", nullable: true),
                    UploadedById = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChunkCount = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IndexedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_AppUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AppUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentChunks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    ChunkIndex = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    Heading = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TokenCount = table.Column<int>(type: "int", nullable: false),
                    HasEmbedding = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentChunks_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "Id", "Code", "CreatedAt", "Description", "Name" },
                values: new object[] { 1, "PRN222", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Advanced C# .NET Core MVC, Entity Framework Core, Real-time Web Apps, and AI Integrations.", "Enterprise Web Application Development" });

            migrationBuilder.InsertData(
                table: "Chapters",
                columns: new[] { "Id", "ChapterNumber", "CreatedAt", "SubjectId", "Summary", "Title" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1, "DbContext, Migration, Lazy Loading, Indexing, and Repository Pattern", "Chapter 1: Entity Framework Core & Data Architecture" },
                    { 2, 2, new DateTime(2025, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Tasks, Async/Await, Task.WhenAll, Thread Pooling, and Concurrency Controls", "Chapter 2: Async & Parallel Programming in C#" },
                    { 3, 3, new DateTime(2025, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Text Extraction, Chunking Strategies, Vector Embeddings, and Retrieval Augmented Generation", "Chapter 3: AI Document Processing & RAG Integration" }
                });

            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "ChapterId", "ChunkCount", "ContentHash", "FileExtension", "FileName", "FileSizeBytes", "IndexedAt", "MimeType", "Status", "StoragePath", "SubjectId", "Title", "UploadedAt", "UploadedById" },
                values: new object[,]
                {
                    { 1, 1, 4, "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", "pdf", "EFCore_BestPractices_PRN222.pdf", 2458000L, new DateTime(2025, 1, 10, 8, 32, 0, 0, DateTimeKind.Utc), "application/pdf", "Ready", "uploads/prn222/EFCore_BestPractices_PRN222.pdf", 1, "EF Core 8 High Performance & Best Practices Guide", new DateTime(2025, 1, 10, 8, 30, 0, 0, DateTimeKind.Utc), 1 },
                    { 2, 2, 3, "8f434346648f6b96df89dda901c5176b10a6d83961dd3c1ac88b59b2dc327aa4", "pptx", "Async_Await_Mastery.pptx", 5120000L, new DateTime(2025, 1, 12, 14, 16, 30, 0, DateTimeKind.Utc), "application/vnd.openxmlformats-officedocument.presentationml.presentation", "Ready", "uploads/prn222/Async_Await_Mastery.pptx", 1, "C# Async Await Deep Dive Lecture Slides", new DateTime(2025, 1, 12, 14, 15, 0, 0, DateTimeKind.Utc), 1 },
                    { 3, 3, 3, "a4567223bfe70911228834900a01299dfa89110298a0011bbcd001299ff01234", "docx", "RAG_Chunking_Spec.docx", 1280000L, new DateTime(2025, 1, 15, 10, 2, 10, 0, DateTimeKind.Utc), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Ready", "uploads/prn222/RAG_Chunking_Spec.docx", 1, "RAG Architecture & Semantic Chunking Specification", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "DocumentChunks",
                columns: new[] { "Id", "ChunkIndex", "Content", "CreatedAt", "DocumentId", "HasEmbedding", "Heading", "PageNumber", "TokenCount" },
                values: new object[,]
                {
                    { 1, 1, "DbContext in Entity Framework Core is designed to be short-lived. Always use AddDbContext with Scoped lifetime in ASP.NET Core web applications.", new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1243), 1, true, "1. DbContext Lifecycle", 1, 120 },
                    { 2, 2, "For read-only queries, calling AsNoTracking() avoids change tracker overhead, boosting performance by up to 30% for large data retrieval.", new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1245), 1, true, "2. AsNoTracking Query Optimization", 3, 145 },
                    { 3, 3, "Ensure foreign keys and frequently filtered columns are indexed in EF Core using HasIndex() in OnModelCreating.", new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1247), 1, true, "3. Explicit Indexing on Foreign Keys", 5, 110 },
                    { 4, 4, "Use EF.CompileAsyncQuery for hot paths to bypass query expression tree parsing overhead.", new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1249), 1, true, "4. Compiled Queries", 8, 95 },
                    { 5, 1, "Async methods yield control back to the calling thread when awaiting an incomplete Task, preventing UI or HTTP worker thread blocking.", new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1250), 2, true, "Slide 2: Thread Pool & Synchronization Context", 2, 130 },
                    { 6, 2, "Async void should only be used for event handlers. Returning Task allows exception propagation and proper async orchestration.", new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1251), 2, true, "Slide 5: Avoid Async Void", 5, 105 },
                    { 7, 3, "Execute independent HTTP or I/O bound requests concurrently using Task.WhenAll to achieve high throughput.", new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1253), 2, true, "Slide 9: Parallel Processing with Task.WhenAll", 9, 125 },
                    { 8, 1, "Semantic chunking splits text at logical structural boundaries such as headings, paragraphs, and lists, preserving original context.", new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1254), 3, true, "Section 1: Semantic Chunking Strategy", 1, 160 },
                    { 9, 2, "Including a 100-token sliding window overlap between adjacent chunks prevents information loss at chunk boundaries during retrieval.", new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1256), 3, true, "Section 2: Sliding Window & Overlap", 2, 150 },
                    { 10, 3, "Generated embeddings are stored alongside chunk index and metadata, allowing fast cosine similarity search in the RAG retrieval pipeline.", new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1257), 3, true, "Section 3: Embedding Vector Storage", 4, 140 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_SubjectId",
                table: "Chapters",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentChunks_DocumentId",
                table: "DocumentChunks",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ChapterId",
                table: "Documents",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SubjectId",
                table: "Documents",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedById",
                table: "Documents",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Code",
                table: "Subjects",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentChunks");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Subjects");
        }
    }
}
