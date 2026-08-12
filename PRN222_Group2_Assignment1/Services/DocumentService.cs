using Microsoft.EntityFrameworkCore;
using PRN222_Group2_Assignment1.Data;
using PRN222_Group2_Assignment1.Models;
using PRN222_Group2_Assignment1.ViewModels;

namespace PRN222_Group2_Assignment1.Services;

public class DocumentService(AppDbContext context, IWebHostEnvironment env) : IDocumentService
{
    private static readonly HashSet<string> AllowedExtensions = ["pdf", "docx", "pptx"];
    private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

    // ── Query / Read ─────────────────────────────────────────────────────────

    public async Task<DocumentManagementViewModel> GetDocumentManagementDataAsync(
        int? subjectId, int? chapterId, string? search, string? fileType)
    {
        var subjects = await context.Subjects
            .Include(s => s.Chapters)
            .Include(s => s.Documents)
            .OrderBy(s => s.Code)
            .ToListAsync();

        var activeSubjectId = subjectId ?? subjects.FirstOrDefault()?.Id;

        var query = context.Documents
            .Include(d => d.Subject)
            .Include(d => d.Chapter)
            .Include(d => d.UploadedBy)
            .Include(d => d.Chunks)
            .AsNoTracking();

        if (activeSubjectId.HasValue)
            query = query.Where(d => d.SubjectId == activeSubjectId.Value);

        if (chapterId.HasValue && chapterId.Value > 0)
            query = query.Where(d => d.ChapterId == chapterId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(d => d.Title.ToLower().Contains(s) || d.FileName.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(fileType))
            query = query.Where(d => d.FileExtension.ToLower() == fileType.Trim().ToLower());

        var filteredDocs = await query.OrderByDescending(d => d.UploadedAt).ToListAsync();
        var allDocs = await context.Documents.AsNoTracking().ToListAsync();
        var chapters = activeSubjectId.HasValue
            ? await context.Chapters.Where(c => c.SubjectId == activeSubjectId.Value).OrderBy(c => c.ChapterNumber).ToListAsync()
            : new List<Chapter>();

        return new DocumentManagementViewModel
        {
            Documents = filteredDocs,
            Subjects = subjects,
            Chapters = chapters,
            SelectedSubjectId = activeSubjectId,
            SelectedChapterId = chapterId,
            SearchQuery = search,
            FileTypeFilter = fileType,
            TotalDocuments = allDocs.Count,
            TotalChunks = allDocs.Sum(d => d.ChunkCount),
            TotalStorageBytes = allDocs.Sum(d => d.FileSizeBytes),
            ReadyDocumentsCount = allDocs.Count(d => d.Status == "Ready"),
            ProcessingDocumentsCount = allDocs.Count(d => d.Status is "Processing" or "Pending")
        };
    }

    public async Task<DocumentDetailJsonViewModel?> GetDocumentChunksAsync(int documentId)
    {
        var doc = await context.Documents
            .Include(d => d.Subject)
            .Include(d => d.Chapter)
            .Include(d => d.Chunks)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (doc is null) return null;

        double mb = doc.FileSizeBytes / (1024.0 * 1024.0);
        string formattedSize = mb >= 1.0 ? $"{mb:F2} MB" : $"{doc.FileSizeBytes / 1024.0:F1} KB";

        return new DocumentDetailJsonViewModel
        {
            Id = doc.Id,
            Title = doc.Title,
            FileName = doc.FileName,
            FileExtension = doc.FileExtension.ToUpperInvariant(),
            FormattedSize = formattedSize,
            Status = doc.Status,
            SubjectName = doc.Subject != null ? $"{doc.Subject.Code} - {doc.Subject.Name}" : "N/A",
            ChapterTitle = doc.Chapter?.Title ?? "General",
            ChunkCount = doc.ChunkCount,
            UploadedAt = doc.UploadedAt.ToString("MMM dd, yyyy HH:mm"),
            Chunks = doc.Chunks.OrderBy(c => c.ChunkIndex).Select(c => new DocumentChunkViewModel
            {
                Id = c.Id,
                ChunkIndex = c.ChunkIndex,
                PageNumber = c.PageNumber,
                Heading = c.Heading,
                Content = c.Content,
                TokenCount = c.TokenCount,
                HasEmbedding = c.HasEmbedding
            }).ToList()
        };
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        var doc = await context.Documents.FindAsync(documentId);
        if (doc is null) return false;

        if (!string.IsNullOrEmpty(doc.StoragePath))
        {
            var physicalPath = Path.Combine(
                env.WebRootPath,
                doc.StoragePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
        }

        context.Documents.Remove(doc);
        await context.SaveChangesAsync();
        return true;
    }

    // ── Upload Pipeline ──────────────────────────────────────────────────────

    public async Task<(bool success, string message, Document? document)> UploadDocumentAsync(
        UploadDocumentViewModel model,
        int uploadedById,
        string uploadsRootPath)
    {
        var file = model.File;

        // ── 1. Validate ──────────────────────────────────────────────────────
        if (file is null || file.Length == 0)
            return (false, "No file provided.", null);

        if (file.Length > MaxFileSizeBytes)
            return (false, $"File exceeds 25 MB limit ({file.Length / (1024.0 * 1024.0):F1} MB).", null);

        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
        if (!AllowedExtensions.Contains(ext))
            return (false, $"File type '.{ext}' is not supported. Only PDF, DOCX, PPTX are allowed.", null);

        // ── 2. Read bytes & compute SHA-256 ──────────────────────────────────
        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            bytes = ms.ToArray();
        }

        var contentHash = DocumentExtractionService.ComputeSha256(bytes);

        // ── 3. Deduplication check ────────────────────────────────────────────
        var existingDoc = await context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ContentHash == contentHash);

        if (existingDoc != null)
            return (false, $"This file has already been uploaded as \"{existingDoc.Title}\".", null);

        // ── 4. Save physical file to disk ────────────────────────────────────
        var safeFileName = $"{Guid.NewGuid():N}_{SanitizeFileName(file.FileName)}";
        var subjectFolder = Path.Combine(uploadsRootPath, $"subject-{model.SubjectId}");
        Directory.CreateDirectory(subjectFolder);

        var physicalPath = Path.Combine(subjectFolder, safeFileName);
        await File.WriteAllBytesAsync(physicalPath, bytes);

        var storagePath = $"uploads/subject-{model.SubjectId}/{safeFileName}";

        // ── 5. Resolve / Create Chapter inline ────────────────────────────────
        int? resolvedChapterId = model.ChapterId;
        if (resolvedChapterId is null && !string.IsNullOrWhiteSpace(model.NewChapterTitle) && model.NewChapterNumber.HasValue)
        {
            var chapter = await GetOrCreateChapterAsync(model.SubjectId, model.NewChapterNumber.Value, model.NewChapterTitle.Trim());
            resolvedChapterId = chapter.Id;
        }

        // ── 6. Insert Document row with Status = "Processing" ─────────────────
        var document = new Document
        {
            Title = string.IsNullOrWhiteSpace(model.Title) ? Path.GetFileNameWithoutExtension(file.FileName) : model.Title.Trim(),
            FileName = file.FileName,
            FileExtension = ext,
            MimeType = file.ContentType,
            FileSizeBytes = file.Length,
            ContentHash = contentHash,
            StoragePath = storagePath,
            SubjectId = model.SubjectId,
            ChapterId = resolvedChapterId,
            UploadedById = uploadedById,
            Status = "Processing",
            ChunkCount = 0,
            UploadedAt = DateTime.UtcNow
        };

        context.Documents.Add(document);
        await context.SaveChangesAsync();

        // ── 7. Extract text + chunk ───────────────────────────────────────────
        try
        {
            var blocks = DocumentExtractionService.ExtractText(bytes, ext);
            var chunks = DocumentChunkingService.Chunk(blocks, document.Id);

            if (chunks.Count > 0)
            {
                context.DocumentChunks.AddRange(chunks);
                document.ChunkCount = chunks.Count;
                document.Status = "Ready";
                document.IndexedAt = DateTime.UtcNow;
            }
            else
            {
                // File parsed but had no extractable text (e.g. scanned image PDF)
                document.Status = "Ready";
                document.ChunkCount = 0;
            }
        }
        catch (Exception ex)
        {
            document.Status = "Failed";
            Console.Error.WriteLine($"[Extraction Error] docId={document.Id}: {ex.Message}");
        }

        await context.SaveChangesAsync();
        return (true, "Document uploaded and indexed successfully.", document);
    }

    // ── Chapter: get-or-create with duplicate guard ──────────────────────────

    public async Task<Chapter> GetOrCreateChapterAsync(int subjectId, int chapterNumber, string title)
    {
        // Check by ChapterNumber first (exact match)
        var existing = await context.Chapters
            .FirstOrDefaultAsync(c => c.SubjectId == subjectId && c.ChapterNumber == chapterNumber);

        if (existing is not null) return existing;

        // Fuzzy title match: same subject, same leading words (first 20 chars)
        var titlePrefix = title.Length > 20 ? title[..20].ToLower() : title.ToLower();
        var fuzzy = await context.Chapters
            .FirstOrDefaultAsync(c => c.SubjectId == subjectId && c.Title.ToLower().StartsWith(titlePrefix));

        if (fuzzy is not null) return fuzzy;

        // Create new chapter
        var chapter = new Chapter
        {
            SubjectId = subjectId,
            ChapterNumber = chapterNumber,
            Title = title,
            CreatedAt = DateTime.UtcNow
        };

        context.Chapters.Add(chapter);
        await context.SaveChangesAsync();
        return chapter;
    }

    public async Task<(bool success, string message)> UpdateSubjectAsync(int subjectId, string name, string? description)
    {
        var subject = await context.Subjects.FindAsync(subjectId);
        if (subject is null) return (false, "Subject not found.");

        subject.Name = name.Trim();
        subject.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        await context.SaveChangesAsync();
        return (true, "Môn học đã được cập nhật thành công.");
    }

    public async Task<(bool success, string message, Subject? subject)> CreateSubjectAsync(string code, string name, string? description)
    {
        var cleanCode = code.Trim().ToUpper();
        var existing = await context.Subjects.FirstOrDefaultAsync(s => s.Code.ToLower() == cleanCode.ToLower());
        if (existing is not null) return (false, $"Môn học có mã \"{cleanCode}\" đã tồn tại.", null);

        var subject = new Subject
        {
            Code = cleanCode,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        context.Subjects.Add(subject);
        await context.SaveChangesAsync();
        return (true, $"Môn học \"{cleanCode} - {subject.Name}\" đã được tạo thành công.", subject);
    }

    public async Task<(bool success, string message)> DeleteSubjectAsync(int subjectId)
    {
        var subject = await context.Subjects
            .Include(s => s.Chapters)
            .Include(s => s.Documents)
                .ThenInclude(d => d.Chunks)
            .FirstOrDefaultAsync(s => s.Id == subjectId);

        if (subject is null) return (false, "Môn học không tồn tại.");

        foreach (var doc in subject.Documents)
        {
            context.DocumentChunks.RemoveRange(doc.Chunks);
        }
        context.Documents.RemoveRange(subject.Documents);
        context.Chapters.RemoveRange(subject.Chapters);
        context.Subjects.Remove(subject);

        await context.SaveChangesAsync();
        return (true, $"Đã xóa môn học \"{subject.Code} - {subject.Name}\" thành công.");
    }

    public async Task<(bool success, string message)> SaveChapterAsync(int subjectId, int? chapterId, int chapterNumber, string title, string? summary)
    {
        if (chapterId.HasValue && chapterId.Value > 0)
        {
            var chapter = await context.Chapters.FindAsync(chapterId.Value);
            if (chapter is null) return (false, "Chapter not found.");

            chapter.ChapterNumber = chapterNumber;
            chapter.Title = title.Trim();
            chapter.Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
            await context.SaveChangesAsync();
            return (true, "Chương đã được cập nhật thành công.");
        }
        else
        {
            var chapter = new Chapter
            {
                SubjectId = subjectId,
                ChapterNumber = chapterNumber,
                Title = title.Trim(),
                Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            context.Chapters.Add(chapter);
            await context.SaveChangesAsync();
            return (true, "Chương mới đã được tạo thành công.");
        }
    }

    public async Task<(bool success, string message)> DeleteChapterAsync(int chapterId)
    {
        var chapter = await context.Chapters.Include(c => c.Documents).FirstOrDefaultAsync(c => c.Id == chapterId);
        if (chapter is null) return (false, "Chapter not found.");

        foreach (var doc in chapter.Documents)
        {
            doc.ChapterId = null;
        }

        context.Chapters.Remove(chapter);
        await context.SaveChangesAsync();
        return (true, "Chương đã được xóa thành công.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(Path.GetFileName(fileName).Select(c => invalid.Contains(c) ? '_' : c));
    }
}
