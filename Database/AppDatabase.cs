using SQLite;
using MyJournals.Models;

namespace MyJournals.Database;

public class AppDatabase
{
    private const string DatabaseFilename = "dailylog.db3";
    private SQLiteAsyncConnection? _connection;
    private bool _isInitialized;
    
    public SQLiteAsyncConnection Connection => _connection ??= new SQLiteAsyncConnection(
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename),
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        await Connection.CreateTableAsync<Mood>();
        await Connection.CreateTableAsync<UserSettings>();
        await Connection.CreateTableAsync<JournalEntry>();

        if (await Connection.Table<Mood>().CountAsync() == 0)
        {
            await SeedMoodsAsync();
        }

        if (await Connection.Table<UserSettings>().CountAsync() == 0)
        {
            await Connection.InsertAsync(new UserSettings { IsProtected = false, Theme = "Light" });
        }

        _isInitialized = true;
    }

    private async Task SeedMoodsAsync()
    {
        var moods = new List<Mood>
        {
            new Mood { MoodType = MoodType.Happy, Category = MoodCategory.Positive },
            new Mood { MoodType = MoodType.Excited, Category = MoodCategory.Positive },
            new Mood { MoodType = MoodType.Relaxed, Category = MoodCategory.Positive },
            new Mood { MoodType = MoodType.Grateful, Category = MoodCategory.Positive },
            new Mood { MoodType = MoodType.Confident, Category = MoodCategory.Positive },
            new Mood { MoodType = MoodType.Calm, Category = MoodCategory.Neutral },
            new Mood { MoodType = MoodType.Thoughtful, Category = MoodCategory.Neutral },
            new Mood { MoodType = MoodType.Curious, Category = MoodCategory.Neutral },
            new Mood { MoodType = MoodType.Nostalgic, Category = MoodCategory.Neutral },
            new Mood { MoodType = MoodType.Bored, Category = MoodCategory.Neutral },
            new Mood { MoodType = MoodType.Sad, Category = MoodCategory.Negative },
            new Mood { MoodType = MoodType.Angry, Category = MoodCategory.Negative },
            new Mood { MoodType = MoodType.Stressed, Category = MoodCategory.Negative },
            new Mood { MoodType = MoodType.Lonely, Category = MoodCategory.Negative },
            new Mood { MoodType = MoodType.Anxious, Category = MoodCategory.Negative }
        };

        await Connection.InsertAllAsync(moods);
    }
}