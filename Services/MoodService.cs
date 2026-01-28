using MyJournals.Database;
using MyJournals.Models;
using MyJournals.Utils;

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
            
        return await _database.Connection.Table<Mood>()
            .OrderBy(m => m.Category)
            .ThenBy(m => m.MoodType)
            .ToListAsync();
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

    public async Task<Dictionary<MoodCategory, List<Mood>>> GetMoodsGroupedByCategoryAsync()
    {
        try
        {
            var allMoods = await GetAllMoodsAsync();
            return allMoods.GroupBy(m => m.Category)
                           .ToDictionary(g => g.Key, g => g.ToList());
        }
        catch (Exception)
        {
            return new Dictionary<MoodCategory, List<Mood>>();
        }
    }

    public string GetMoodIcon(MoodCategory category)
    {
        return MoodHelpers.GetCategoryIcon(category);
    }

    public string GetMoodColor(MoodCategory category)
    {
        return MoodHelpers.GetCategoryColor(category);
    }

    public string GetMoodEmoji(MoodType moodType)
    {
        return MoodHelpers.GetMoodEmoji(moodType);
    }
}
