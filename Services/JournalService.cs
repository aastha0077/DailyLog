using MyJournals.Database;
using MyJournals.Models;
using MyJournals.Utils;
using SQLite;

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

        var targetDate = date.Date;
        var entries = await _database.Connection.Table<JournalEntry>().ToListAsync();
        return entries.FirstOrDefault(e =>
            e.EntryDate.Year == targetDate.Year &&
            e.EntryDate.Month == targetDate.Month &&
            e.EntryDate.Day == targetDate.Day);
    }
    
    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        return await _database.Connection.GetAsync<JournalEntry>(id);
    }
    
    public async Task<List<JournalEntry>> GetAllEntriesAsync()
    {
        return await _database.Connection.Table<JournalEntry>()
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }
    
    public async Task<List<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {

        var entries = await _database.Connection.Table<JournalEntry>().ToListAsync();
        return entries.Where(e => e.EntryDate.Date >= startDate.Date && e.EntryDate.Date <= endDate.Date)
                     .OrderByDescending(e => e.EntryDate)
                     .ToList();
    }
    
    public async Task<JournalEntry> CreateOrUpdateEntryAsync(JournalEntry entry)
    {

        entry.UpdatedAt = DateTime.Now;
        entry.WordCount = StringHelpers.CountWords(entry.Content);

        var existingEntry = await GetEntryByDateAsync(entry.EntryDate);
        
        if (existingEntry != null)
        {
            entry.Id = existingEntry.Id;
            entry.CreatedAt = existingEntry.CreatedAt;
            await _database.Connection.UpdateAsync(entry);
        }
        else
        {
            entry.CreatedAt = DateTime.Now;
            await _database.Connection.InsertAsync(entry);
        }

        return entry;
    }
    
    public async Task DeleteEntryAsync(int id)
    {
        await _database.Connection.DeleteAsync<JournalEntry>(id);
    }
    
    public List<string> GetTags(JournalEntry entry)
    {
        return StringHelpers.ExtractTags(entry.Tags);
    }
    
    public void SetTags(JournalEntry entry, List<string> tags)
    {
        entry.Tags = StringHelpers.ConvertTagsToJson(tags?.Distinct().ToList() ?? new List<string>());
    }
    
    public async Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm, DateTime? startDate = null, DateTime? endDate = null, List<int>? moodIds = null, List<string>? tags = null)
    {

        var entries = await GetAllEntriesAsync();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            entries = entries.Where(e => 
                e.Title.ToLower().Contains(searchTerm) || 
                e.Content.ToLower().Contains(searchTerm)).ToList();
        }
        
        if (startDate.HasValue)
        {
            entries = entries.Where(e => e.EntryDate.Date >= startDate.Value.Date).ToList();
        }
        
        if (endDate.HasValue)
        {
            entries = entries.Where(e => e.EntryDate.Date <= endDate.Value.Date).ToList();
        }
        
        if (moodIds != null && moodIds.Any())
        {
            entries = entries.Where(e => 
                moodIds.Contains(e.PrimaryMoodId) ||
                (e.SecondaryMood1Id.HasValue && moodIds.Contains(e.SecondaryMood1Id.Value)) ||
                (e.SecondaryMood2Id.HasValue && moodIds.Contains(e.SecondaryMood2Id.Value))
            ).ToList();
        }
        
        if (tags != null && tags.Any())
        {
            entries = entries.Where(e => 
            {
                var entryTags = GetTags(e);
                return tags.Any(tag => entryTags.Contains(tag, StringComparer.OrdinalIgnoreCase));
            }).ToList();
        }
        
        return entries;
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
