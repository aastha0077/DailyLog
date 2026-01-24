using MyJournals.Models;
using MyJournals.Services;
using System.Text;

namespace MyJournals.Services;

public class ExportService
{
    private readonly JournalService _journalService;
    
    public ExportService(JournalService journalService)
    {
        _journalService = journalService;
    }
    
    public async Task<string> GeneratePdfContentAsync(DateTime startDate, DateTime endDate)
    {
        var entries = await _journalService.GetEntriesByDateRangeAsync(startDate, endDate);
        var sb = new StringBuilder();
        
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8' />");
        sb.AppendLine("<title>My Journals Export</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
        sb.AppendLine(".entry { margin-bottom: 30px; page-break-inside: avoid; }");
        sb.AppendLine(".entry-header { border-bottom: 2px solid #333; padding-bottom: 10px; margin-bottom: 15px; }");
        sb.AppendLine(".entry-date { font-size: 18px; font-weight: bold; }");
        sb.AppendLine(".entry-title { font-size: 16px; margin-top: 5px; }");
        sb.AppendLine(".entry-content { margin-top: 15px; line-height: 1.6; }");
        sb.AppendLine(".entry-meta { color: #666; font-size: 12px; margin-top: 10px; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h1>My Journals Export</h1>");
        sb.AppendLine($"<p>Export Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}</p>");
        sb.AppendLine($"<p>Total Entries: {entries.Count}</p>");
        sb.AppendLine("<hr />");
        
        foreach (var entry in entries)
        {
            sb.AppendLine("<div class='entry'>");
            sb.AppendLine($"<div class='entry-header'>");
            sb.AppendLine($"<div class='entry-date'>{entry.EntryDate:MMMM dd, yyyy}</div>");
            if (!string.IsNullOrEmpty(entry.Title))
            {
                sb.AppendLine($"<div class='entry-title'>{System.Security.SecurityElement.Escape(entry.Title)}</div>");
            }
            sb.AppendLine("</div>");
            
            sb.AppendLine($"<div class='entry-content'>{System.Security.SecurityElement.Escape(entry.Content).Replace("\n", "<br />")}</div>");
            
            sb.AppendLine("<div class='entry-meta'>");
            sb.AppendLine($"Created: {entry.CreatedAt:yyyy-MM-dd HH:mm} | Updated: {entry.UpdatedAt:yyyy-MM-dd HH:mm}");
            sb.AppendLine($" | Words: {entry.WordCount}");
            if (!string.IsNullOrEmpty(entry.Category))
            {
                sb.AppendLine($" | Category: {System.Security.SecurityElement.Escape(entry.Category)}");
            }
            var tags = _journalService.GetTags(entry);
            if (tags.Count > 0)
            {
                sb.AppendLine($" | Tags: {string.Join(", ", tags.Select(t => System.Security.SecurityElement.Escape(t)))}");
            }
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div>");
        }
        
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        
        return sb.ToString();
    }
}
