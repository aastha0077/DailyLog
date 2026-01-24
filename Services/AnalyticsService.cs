using MyJournals.Database;
using MyJournals.Models;

namespace MyJournals.Services;

public class AnalyticsService
{
    private readonly JournalService _journalService;
    private readonly AppDatabase _database;
    
    public AnalyticsService(JournalService journalService, AppDatabase database)
    {
        _journalService = journalService;
        _database = database;
    }
    
    public async Task<StreakInfo> GetStreakInfoAsync()
    {
        try
        {
            var entries = await _journalService.GetAllEntriesAsync();
            if (entries.Count == 0)
            {
                return new StreakInfo();
            }
        
        var entryDates = entries.Select(e => e.EntryDate.Date).OrderByDescending(d => d).ToList();
        var today = DateTime.Now.Date;
        var streakInfo = new StreakInfo();
        
        // Calculate current streak
        var currentStreak = 0;
        var checkDate = today;
        
        while (entryDates.Contains(checkDate))
        {
            currentStreak++;
            checkDate = checkDate.AddDays(-1);
        }
        
        // If there's no entry today, check if yesterday started a streak
        if (currentStreak == 0 && entryDates.Count > 0)
        {
            checkDate = today.AddDays(-1);
            while (entryDates.Contains(checkDate))
            {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }
        }
        
        streakInfo.CurrentStreak = currentStreak;
        
        // Calculate longest streak
        var longestStreak = 0;
        var tempStreak = 0;
        var sortedDates = entryDates.OrderBy(d => d).ToList();
        
        for (int i = 0; i < sortedDates.Count; i++)
        {
            if (i == 0 || sortedDates[i] == sortedDates[i - 1].AddDays(1))
            {
                tempStreak++;
            }
            else
            {
                longestStreak = Math.Max(longestStreak, tempStreak);
                tempStreak = 1;
            }
        }
        longestStreak = Math.Max(longestStreak, tempStreak);
        
        streakInfo.LongestStreak = longestStreak;
        
        // Find missed days (gaps in entries)
        if (entryDates.Count > 0)
        {
            var minDate = entryDates.Min();
            var maxDate = entryDates.Max();
            var missedDays = new List<DateTime>();
            
            for (var date = minDate; date <= maxDate; date = date.AddDays(1))
            {
                if (!entryDates.Contains(date))
                {
                    missedDays.Add(date);
                }
            }
            
            streakInfo.MissedDays = missedDays;
        }

        return streakInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating streak info: {ex.Message}");
            return new StreakInfo();
        }
    }
    
    public async Task<List<MoodDistribution>> GetMoodDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var entries = startDate.HasValue && endDate.HasValue
            ? await _journalService.GetEntriesByDateRangeAsync(startDate.Value, endDate.Value)
            : await _journalService.GetAllEntriesAsync();
        
        var moodCounts = new Dictionary<MoodCategory, int>
        {
            { MoodCategory.Positive, 0 },
            { MoodCategory.Neutral, 0 },
            { MoodCategory.Negative, 0 }
        };
        
        var allMoods = await _database.Connection.Table<Mood>().ToListAsync();
        var moodDict = allMoods.ToDictionary(m => m.Id, m => m.Category);
        
        foreach (var entry in entries)
        {
            if (moodDict.TryGetValue(entry.PrimaryMoodId, out var category))
            {
                moodCounts[category]++;
            }
        }
        
        var total = entries.Count;
        return moodCounts.Select(kvp => new MoodDistribution
        {
            Category = kvp.Key,
            Count = kvp.Value,
            Percentage = total > 0 ? (double)kvp.Value / total * 100 : 0
        }).ToList();
    }
    
    public async Task<MoodType?> GetMostFrequentMoodAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var entries = startDate.HasValue && endDate.HasValue
            ? await _journalService.GetEntriesByDateRangeAsync(startDate.Value, endDate.Value)
            : await _journalService.GetAllEntriesAsync();
        
        var moodCounts = new Dictionary<int, int>();
        
        foreach (var entry in entries)
        {
            moodCounts[entry.PrimaryMoodId] = moodCounts.GetValueOrDefault(entry.PrimaryMoodId, 0) + 1;
        }
        
        if (moodCounts.Count == 0)
            return null;
        
        var mostFrequentMoodId = moodCounts.OrderByDescending(kvp => kvp.Value).First().Key;
        var mood = await _database.Connection.GetAsync<Mood>(mostFrequentMoodId);
        return mood?.MoodType;
    }
    
    public async Task<List<TagUsage>> GetMostUsedTagsAsync(int topN = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        var entries = startDate.HasValue && endDate.HasValue
            ? await _journalService.GetEntriesByDateRangeAsync(startDate.Value, endDate.Value)
            : await _journalService.GetAllEntriesAsync();
        
        var tagCounts = new Dictionary<string, int>();
        var totalEntries = entries.Count;
        
        foreach (var entry in entries)
        {
            var tags = _journalService.GetTags(entry);
            foreach (var tag in tags)
            {
                tagCounts[tag] = tagCounts.GetValueOrDefault(tag, 0) + 1;
            }
        }
        
        return tagCounts.OrderByDescending(kvp => kvp.Value)
            .Take(topN)
            .Select(kvp => new TagUsage
            {
                Tag = kvp.Key,
                Count = kvp.Value,
                Percentage = totalEntries > 0 ? (double)kvp.Value / totalEntries * 100 : 0
            })
            .ToList();
    }
    
    public async Task<List<TagUsage>> GetTagBreakdownAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var entries = startDate.HasValue && endDate.HasValue
            ? await _journalService.GetEntriesByDateRangeAsync(startDate.Value, endDate.Value)
            : await _journalService.GetAllEntriesAsync();
        
        var tagCounts = new Dictionary<string, int>();
        var totalEntries = entries.Count;
        
        foreach (var entry in entries)
        {
            var tags = _journalService.GetTags(entry);
            foreach (var tag in tags)
            {
                tagCounts[tag] = tagCounts.GetValueOrDefault(tag, 0) + 1;
            }
        }
        
        return tagCounts.Select(kvp => new TagUsage
        {
            Tag = kvp.Key,
            Count = kvp.Value,
            Percentage = totalEntries > 0 ? (double)kvp.Value / totalEntries * 100 : 0
        })
        .OrderByDescending(t => t.Count)
        .ToList();
    }
    
    public async Task<List<WordCountTrend>> GetWordCountTrendsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var entries = startDate.HasValue && endDate.HasValue
            ? await _journalService.GetEntriesByDateRangeAsync(startDate.Value, endDate.Value)
            : await _journalService.GetAllEntriesAsync();
        
        if (entries.Count == 0)
            return new List<WordCountTrend>();
        
        var trends = entries.OrderBy(e => e.EntryDate)
            .Select(e => new WordCountTrend
            {
                Date = e.EntryDate,
                WordCount = e.WordCount
            })
            .ToList();
        
        // Calculate running average
        var runningTotal = 0;
        for (int i = 0; i < trends.Count; i++)
        {
            runningTotal += trends[i].WordCount;
            trends[i].AverageWordCount = (double)runningTotal / (i + 1);
        }
        
        return trends;
    }
}
