using MyJournals.Models;
using SkiaSharp;

namespace MyJournals.Services;

public class ExportService
{
    private readonly JournalService _journalService;
    
    public ExportService(JournalService journalService)
    {
        _journalService = journalService;
    }
    
    public async Task<string> ExportToPdfAsync(DateTime startDate, DateTime endDate)
    {
        var entries = await _journalService.GetEntriesByDateRangeAsync(startDate, endDate);
        var fileName = $"journal_export_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        
        // Use Documents folder on Mac for better visibility
        string baseDir = FileSystem.CacheDirectory;
        #if MACCATALYST
        baseDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        #endif
        
        var filePath = Path.Combine(baseDir, fileName);
        
        using (var stream = new SKFileWStream(filePath))
        using (var document = SKDocument.CreatePdf(stream))
        {
            var paint = new SKPaint { TextSize = 12, Color = SKColors.Black, IsAntialias = true };
            var titlePaint = new SKPaint { TextSize = 24, Color = SKColors.DarkBlue, IsAntialias = true };

            using (var canvas = document.BeginPage(595, 842))
            {
                float y = 50;
                float x = 50;
                float width = 495;

                canvas.DrawText("Journal Entries", x, y, titlePaint);
                y += 40;
                
                canvas.DrawText($"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", x, y, paint);
                y += 30;

                if (entries.Count == 0)
                {
                    canvas.DrawText("No entries found.", x, y, paint);
                }
                else
                {
                    foreach (var entry in entries)
                    {
                        if (y > 750) break; 

                        canvas.DrawText(entry.EntryDate.ToString("yyyy-MM-dd"), x, y, new SKPaint { TextSize = 14, Color = SKColors.Gray, IsAntialias = true });
                        y += 20;

                        if (!string.IsNullOrEmpty(entry.Title))
                        {
                            canvas.DrawText(entry.Title, x, y, new SKPaint { TextSize = 13, IsAntialias = true });
                            y += 18;
                        }

                        var lines = WrapText(entry.Content ?? "", paint, width);
                        foreach (var line in lines)
                        {
                            if (y > 800) break;
                            canvas.DrawText(line, x, y, paint);
                            y += 15;
                        }
                        y += 20;
                    }
                }
                document.EndPage();
            }
            document.Close();
        }
        return filePath;
    }

    private List<string> WrapText(string text, SKPaint paint, float maxWidth)
    {
        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
            if (paint.MeasureText(testLine) <= maxWidth)
                currentLine = testLine;
            else
            {
                lines.Add(currentLine);
                currentLine = word;
            }
        }
        if (!string.IsNullOrEmpty(currentLine)) lines.Add(currentLine);
        return lines;
    }
}
