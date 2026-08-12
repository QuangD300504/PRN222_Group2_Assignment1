using PRN222_Group2_Assignment1.Models;

namespace PRN222_Group2_Assignment1.Services;

/// <summary>
/// Chunks a list of TextBlocks into DocumentChunk records.
/// Strategy: Each TextBlock (page/slide) is chunked cleanly.
/// If a block fits within MaxChars, it produces 1 chunk.
/// If a block exceeds MaxChars, it splits cleanly using a sliding window.
/// </summary>
public static class DocumentChunkingService
{
    private const int MaxChars = 3000;    // ~750 tokens
    private const int OverlapChars = 300; // ~75 tokens overlap

    public static List<DocumentChunk> Chunk(List<DocumentExtractionService.TextBlock> blocks, int documentId)
    {
        var chunks = new List<DocumentChunk>();
        int index = 1;
        string? lastHeading = null;

        foreach (var block in blocks)
        {
            if (string.IsNullOrWhiteSpace(block.Text) || block.Text.Trim().Length < 10) continue;

            string heading = !string.IsNullOrWhiteSpace(block.Heading)
                ? block.Heading.Trim()
                : (lastHeading ?? $"Page {block.Page}");

            lastHeading = heading;

            var windows = SplitIntoWindows(block.Text.Trim(), MaxChars, OverlapChars);

            foreach (var windowText in windows)
            {
                if (string.IsNullOrWhiteSpace(windowText)) continue;

                chunks.Add(new DocumentChunk
                {
                    DocumentId = documentId,
                    ChunkIndex = index++,
                    PageNumber = block.Page,
                    Heading = heading,
                    Content = windowText.Trim(),
                    TokenCount = EstimateTokens(windowText),
                    HasEmbedding = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        return chunks;
    }

    private static List<string> SplitIntoWindows(string text, int maxChars, int overlapChars)
    {
        var results = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return results;

        // If whole text fits in one window, return 1 chunk and exit immediately
        if (text.Length <= maxChars)
        {
            results.Add(text);
            return results;
        }

        int start = 0;
        while (start < text.Length)
        {
            int end = Math.Min(start + maxChars, text.Length);

            if (end < text.Length)
            {
                int paraBreak = text.LastIndexOf("\n\n", end, Math.Min(end - start, overlapChars * 2));
                int sentBreak = text.LastIndexOf(". ", end, Math.Min(end - start, overlapChars * 2));
                int wordBreak = text.LastIndexOf(' ', end, Math.Min(end - start, overlapChars));

                int breakAt = paraBreak > start ? paraBreak
                            : sentBreak > start ? sentBreak + 1
                            : wordBreak > start ? wordBreak
                            : end;

                end = breakAt;
            }

            string slice = text[start..end].Trim();
            if (!string.IsNullOrWhiteSpace(slice))
            {
                results.Add(slice);
            }

            if (end >= text.Length) break;

            // Ensure forward progress past current chunk minus overlap
            int nextStart = end - overlapChars;
            if (nextStart <= start)
            {
                nextStart = end;
            }
            start = nextStart;
        }

        return results;
    }

    private static int EstimateTokens(string text) => Math.Max(1, text.Length / 4);
}
