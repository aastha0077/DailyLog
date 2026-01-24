using MyJournals.Database;
using MyJournals.Models;

namespace MyJournals.Services;

public class MoodService
{
    private readonly AppDatabase _database;
    
    public MoodService(AppDatabase database)
    {
        _database = database;
    }
    
    public async Task<List<Mood>> GetAllMoodsAsync()
    {
        try
        {
            return await _database.Connection.Table<Mood>()
                .OrderBy(m => m.Category)
                .ThenBy(m => m.MoodType)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting moods: {ex.Message}");
            return new List<Mood>();
        }
    }
    
    public async Task<List<Mood>> GetMoodsByCategoryAsync(MoodCategory category)
    {
        return await _database.Connection.Table<Mood>()
            .Where(m => m.Category == category)
            .OrderBy(m => m.MoodType)
            .ToListAsync();
    }
    
    public async Task<Mood?> GetMoodByIdAsync(int id)
    {
        return await _database.Connection.GetAsync<Mood>(id);
    }
}
