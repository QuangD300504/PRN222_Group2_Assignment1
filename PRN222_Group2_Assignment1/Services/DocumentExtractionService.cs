using System.Security.Cryptography;
using System.Text;
using PdfPigDoc = UglyToad.PdfPig.PdfDocument;
using WinPdfDoc = Windows.Data.Pdf.PdfDocument;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using PRN222_Group2_Assignment1.Models;

namespace PRN222_Group2_Assignment1.Services;

/// <summary>
/// Extracts text from PDF, DOCX, and PPTX into structured page-aware blocks.
/// Returns a flat list of (pageNumber, heading, text) tuples ready for chunking.
/// Includes Windows Native PDF Renderer + OCR Fallback for scanned/vector-drawn PDFs.
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
        try
        {
            using var doc = PdfPigDoc.Open(bytes);

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
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PdfPig Extraction Warning] {ex.Message}");
        }

        // If standard text extraction produced 0 words (e.g. vector-drawn PDF or scanned bitmap PDF),
        // fallback to Windows Native PDF Renderer + Windows.Media.Ocr Engine!
        if (blocks.Count == 0 || blocks.All(b => string.IsNullOrWhiteSpace(b.Text)))
        {
            var ocrBlocks = ExtractPdfOcrFallback(bytes);
            if (ocrBlocks.Count > 0)
                return ocrBlocks;
        }

        return blocks;
    }

    private static List<TextBlock> ExtractPdfOcrFallback(byte[] bytes)
    {
        var blocks = new List<TextBlock>();
        try
        {
            var task = Task.Run(async () =>
            {
                using var stream = new MemoryStream(bytes);
                using var randomAccessStream = stream.AsRandomAccessStream();
                var pdfDoc = await WinPdfDoc.LoadFromStreamAsync(randomAccessStream);

                var ocrEngine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en-US"))
                                ?? OcrEngine.TryCreateFromUserProfileLanguages();

                if (ocrEngine is null) return blocks;

                for (uint i = 0; i < pdfDoc.PageCount; i++)
                {
                    using var page = pdfDoc.GetPage(i);
                    using var imgStream = new InMemoryRandomAccessStream();
                    await page.RenderToStreamAsync(imgStream);

                    var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(imgStream);
                    using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);
                    var text = ocrResult.Text?.Trim();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        string? heading = lines.Length > 0 && IsLikelyHeading(lines[0]) ? lines[0] : null;
                        blocks.Add(new TextBlock((int)(i + 1), heading, text));
                    }
                }
                return blocks;
            });
            return task.GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PDF OCR Fallback Error] {ex.Message}");
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
