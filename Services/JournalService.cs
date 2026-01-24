using MyJournals.Database;
using MyJournals.Models;
using SQLite;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MyJournals.Services;

public class JournalService
{
    private readonly AppDatabase _database;
    
    public JournalService(AppDatabase database)
    {
        _database = database;
    }
    
    public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
    {
        try
        {
            // SQLite-Net-PCL can't handle DateTime.Date comparisons, so we compare year, month, day separately
            var targetDate = date.Date;
            var entries = await _database.Connection.Table<JournalEntry>().ToListAsync();
            return entries.FirstOrDefault(e =>
                e.EntryDate.Year == targetDate.Year &&
                e.EntryDate.Month == targetDate.Month &&
                e.EntryDate.Day == targetDate.Day);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting entry by date: {ex.Message}");
            return null;
        }
    }
    
    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        return await _database.Connection.GetAsync<JournalEntry>(id);
    }
    
    public async Task<List<JournalEntry>> GetAllEntriesAsync()
    {
        try
        {
            return await _database.Connection.Table<JournalEntry>()
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting all entries: {ex.Message}");
            return new List<JournalEntry>();
        }
    }
    
    public async Task<List<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date;
            var allEntries = await _database.Connection.Table<JournalEntry>().ToListAsync();
            return allEntries
                .Where(e => e.EntryDate.Date >= start && e.EntryDate.Date <= end)
                .OrderByDescending(e => e.EntryDate)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting entries by date range: {ex.Message}");
            return new List<JournalEntry>();
        }
    }
    
    public async Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _database.Connection.Table<JournalEntry>()
            .Where(e => e.Title.ToLower().Contains(term) || e.Content.ToLower().Contains(term))
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }
    
    public async Task<List<JournalEntry>> FilterEntriesAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? moodId = null,
        string? tag = null)
    {
        try
        {
            var allEntries = await _database.Connection.Table<JournalEntry>().ToListAsync();

            var results = allEntries.AsQueryable();

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                results = results.Where(e => e.EntryDate.Date >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date;
                results = results.Where(e => e.EntryDate.Date <= end);
            }

            if (moodId.HasValue)
            {
                results = results.Where(e => e.PrimaryMoodId == moodId.Value ||
                                           e.SecondaryMood1Id == moodId.Value ||
                                           e.SecondaryMood2Id == moodId.Value);
            }

            var filteredResults = results.OrderByDescending(e => e.EntryDate).ToList();

            if (!string.IsNullOrEmpty(tag))
            {
                filteredResults = filteredResults.Where(e => GetTags(e).Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            return filteredResults;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error filtering entries: {ex.Message}");
            return new List<JournalEntry>();
        }
    }
    
    public async Task<JournalEntry> CreateOrUpdateEntryAsync(JournalEntry entry)
    {
        var existing = await GetEntryByDateAsync(entry.EntryDate);
        
        if (existing != null)
        {
            // Update existing entry
            entry.Id = existing.Id;
            entry.CreatedAt = existing.CreatedAt;
            entry.UpdatedAt = DateTime.Now;
            entry.WordCount = CountWords(entry.Content);
            await _database.Connection.UpdateAsync(entry);
            return entry;
        }
        else
        {
            // Create new entry
            entry.CreatedAt = DateTime.Now;
            entry.UpdatedAt = DateTime.Now;
            entry.WordCount = CountWords(entry.Content);
            await _database.Connection.InsertAsync(entry);
            return entry;
        }
    }
    
    public async Task<bool> DeleteEntryAsync(int id)
    {
        var entry = await GetEntryByIdAsync(id);
        if (entry != null)
        {
            await _database.Connection.DeleteAsync(entry);
            return true;
        }
        return false;
    }
    
    public async Task<bool> DeleteEntryByDateAsync(DateTime date)
    {
        var entry = await GetEntryByDateAsync(date);
        if (entry != null)
        {
            await _database.Connection.DeleteAsync(entry);
            return true;
        }
        return false;
    }
    
    public List<string> GetTags(JournalEntry entry)
    {
        if (string.IsNullOrEmpty(entry.Tags))
            return new List<string>();
        
        try
        {
            return JsonSerializer.Deserialize<List<string>>(entry.Tags) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
    
    public void SetTags(JournalEntry entry, List<string> tags)
    {
        entry.Tags = JsonSerializer.Serialize(tags);
    }
    
    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        
        // Remove markdown formatting for word count
        var plainText = Regex.Replace(text, @"[#*_`\[\]()]", " ");
        var words = plainText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        return words.Length;
    }
    
    public async Task<List<JournalEntry>> GetPaginatedEntriesAsync(int page, int pageSize)
    {
        return await _database.Connection.Table<JournalEntry>()
            .OrderByDescending(e => e.EntryDate)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<int> GetTotalEntryCountAsync()
    {
        return await _database.Connection.Table<JournalEntry>().CountAsync();
    }
}
