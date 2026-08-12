using System.Security.Cryptography;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using PRN222_Group2_Assignment1.Models;

namespace PRN222_Group2_Assignment1.Services;

/// <summary>
/// Extracts text from PDF, DOCX, and PPTX into structured page-aware blocks.
/// Returns a flat list of (pageNumber, heading, text) tuples ready for chunking.
/// </summary>
public static class DocumentExtractionService
{
    public record TextBlock(int Page, string? Heading, string Text);

    public static string ComputeSha256(byte[] bytes)
    {
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    public static List<TextBlock> ExtractText(byte[] bytes, string extension)
    {
        return extension.ToLower() switch
        {
            "pdf"  => ExtractPdf(bytes),
            "docx" => ExtractDocx(bytes),
            "pptx" => ExtractPptx(bytes),
            _      => new List<TextBlock>()
        };
    }

    // ── PDF ──────────────────────────────────────────────────────────────────
    private static List<TextBlock> ExtractPdf(byte[] bytes)
    {
        var blocks = new List<TextBlock>();
        using var doc = PdfDocument.Open(bytes);

        foreach (var page in doc.GetPages())
        {
            // Group words into lines by Y-coordinate bucket (~3 pt line threshold)
            var lines = page.GetWords()
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom / 3.0, 0) * 3)
                .OrderByDescending(g => g.Key)
                .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            if (lines.Count == 0) continue;

            string? heading = IsLikelyHeading(lines[0]) ? lines[0] : null;
            string text = string.Join("\n", lines); // Include full line text so headings are preserved

            if (!string.IsNullOrWhiteSpace(text))
                blocks.Add(new TextBlock(page.Number, heading, text));
        }

        return blocks;
    }

    // ── DOCX ─────────────────────────────────────────────────────────────────
    private static List<TextBlock> ExtractDocx(byte[] bytes)
    {
        var blocks = new List<TextBlock>();
        using var ms = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(ms, false);

        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null) return blocks;

        string? currentHeading = null;
        var accum = new StringBuilder();
        int page = 1;

        foreach (var para in body.Elements<Paragraph>())
        {
            var style = para.ParagraphProperties?.ParagraphStyleId?.Val?.Value ?? "";
            string text = para.InnerText.Trim();
            if (string.IsNullOrEmpty(text)) continue;

            bool isHeading = style.StartsWith("Heading", StringComparison.OrdinalIgnoreCase)
                          || style is "1" or "2" or "3";

            if (isHeading)
            {
                if (accum.Length > 0)
                {
                    blocks.Add(new TextBlock(page, currentHeading, accum.ToString().Trim()));
                    accum.Clear();
                    page++;
                }
                currentHeading = text;
                accum.AppendLine(text);
            }
            else
            {
                accum.AppendLine(text);
            }
        }

        if (accum.Length > 0)
            blocks.Add(new TextBlock(page, currentHeading, accum.ToString().Trim()));

        return blocks;
    }

    // ── PPTX ─────────────────────────────────────────────────────────────────
    private static List<TextBlock> ExtractPptx(byte[] bytes)
    {
        var blocks = new List<TextBlock>();
        using var ms = new MemoryStream(bytes);
        using var prs = PresentationDocument.Open(ms, false);

        var presentationPart = prs.PresentationPart;
        if (presentationPart is null) return blocks;

        var slideIds = presentationPart.Presentation.SlideIdList?.ChildElements
            .OfType<SlideId>().ToList() ?? new();

        for (int i = 0; i < slideIds.Count; i++)
        {
            var rId = slideIds[i].RelationshipId?.Value;
            if (rId is null) continue;

            var slidePart = (SlidePart)presentationPart.GetPartById(rId);

            // 1. Title shape placeholder extraction
            string? heading = null;
            var titleShape = slidePart.Slide.Descendants<Shape>()
                .FirstOrDefault(s => {
                    var phType = s.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?
                        .GetFirstChild<PlaceholderShape>()?.Type?.Value;
                    return phType == PlaceholderValues.Title || phType == PlaceholderValues.CenteredTitle;
                });

            if (titleShape != null)
            {
                var titleText = string.Join(" ", titleShape.Descendants<A.Text>()
                    .Select(t => t.Text?.Trim())
                    .Where(t => !string.IsNullOrEmpty(t)));

                if (!string.IsNullOrWhiteSpace(titleText))
                {
                    heading = titleText;
                }
            }

            // 2. All paragraph texts on slide
            var paragraphs = slidePart.Slide.Descendants<A.Paragraph>()
                .Select(p => p.InnerText?.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();

            if (paragraphs.Count == 0) continue;

            if (string.IsNullOrWhiteSpace(heading))
            {
                heading = paragraphs.FirstOrDefault();
            }

            string fullSlideContent = string.Join("\n", paragraphs);

            blocks.Add(new TextBlock(i + 1, heading, fullSlideContent));
        }

        return blocks;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static bool IsLikelyHeading(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return false;
        if (line.Length > 120) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(line, @"^(\d+[\.\):]|chapter|section|part|slide|unit|lab|module)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            || (line.Length < 60 && line == line.ToUpper() && line.Any(char.IsLetter));
    }
}
