using SQLite;
using MyJournals.Models;
using System.Diagnostics;

namespace MyJournals.Database;

public class AppDatabase
{
    private const string DatabaseFilename = "dailylog.db3";
    private SQLiteAsyncConnection? _connection;
    
    public SQLiteAsyncConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                try
                {
                    var databaseDirectory = FileSystem.AppDataDirectory;
                    var databasePath = Path.Combine(databaseDirectory, DatabaseFilename);

                    // Ensure the directory exists
                    Directory.CreateDirectory(databaseDirectory);

                    Debug.WriteLine($"Database path: {databasePath}");
                    Debug.WriteLine($"Database directory exists: {Directory.Exists(databaseDirectory)}");
                    Console.WriteLine($"Initializing database connection at: {databasePath}");

                    _connection = new SQLiteAsyncConnection(databasePath, 
                        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

                    // Verify the database file was created
                    Debug.WriteLine($"Database file exists after connection: {File.Exists(databasePath)}");
                    Console.WriteLine($"Database connection created successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR creating database connection: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    throw new Exception($"Failed to create database connection: {ex.Message}", ex);
                }
            }
            return _connection;
        }
    }
    
    private bool _isInitialized = false;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        try
        {
            Debug.WriteLine($"Starting database initialization...");
            Debug.WriteLine($"Database file path: {databasePath}");

            // Ensure database file exists by triggering connection creation
            var conn = Connection;
            Debug.WriteLine($"Database connection created successfully");

            // Verify file exists
            // Verify file exists check removed as it may not exist until first write
            Debug.WriteLine("Proceeding to create tables...");

            // FileInfo check moved to after table creation
            Debug.WriteLine("Skipping file info check until tables are created...");

            // Create tables one by one to catch any specific issues
            await Connection.CreateTableAsync<Mood>();
            Debug.WriteLine("Moods table created successfully");
            // Handle UserSettings table migration
            await MigrateUserSettingsTableAsync();
            Debug.WriteLine("UserSettings table migration completed");

            await Connection.CreateTableAsync<JournalEntry>();
            Debug.WriteLine("JournalEntries table created successfully");

            // Initialize moods if empty
            if (await Connection.Table<Mood>().CountAsync() == 0)
            {
                await SeedMoodsAsync();
            }

            // Initialize user settings if empty
            if (await Connection.Table<UserSettings>().CountAsync() == 0)
            {
                await Connection.InsertAsync(new UserSettings { IsProtected = false, Theme = "Light" });
            }

            // Final verification
            var finalMoodCount = await Connection.Table<Mood>().CountAsync();
            var finalSettingsCount = await Connection.Table<UserSettings>().CountAsync();
            Debug.WriteLine($"Final verification - Moods: {finalMoodCount}, Settings: {finalSettingsCount}");

            _isInitialized = true;
            Debug.WriteLine("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Database initialization error: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            // If there's an error, try to delete the database and start fresh
            try
            {
                Debug.WriteLine("Attempting database recreation...");
                await Connection.CloseAsync();
                _connection = null;

                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                    Debug.WriteLine("Database file deleted, will recreate on next run");
                }

                // Reinitialize with a fresh database
                var newConn = Connection; // This will recreate the connection
                await Connection.CreateTableAsync<Mood>();
                await Connection.CreateTableAsync<UserSettings>();
                await Connection.CreateTableAsync<JournalEntry>();
                await SeedMoodsAsync();
                await Connection.InsertAsync(new UserSettings
                {
                    IsProtected = false,
                    Theme = "Light"
                });
                Debug.WriteLine("Database recreated successfully");
            }
            catch (Exception retryEx)
            {
                Debug.WriteLine($"Database recreation failed: {retryEx.Message}");
                Debug.WriteLine($"Recreation stack trace: {retryEx.StackTrace}");
                throw;
            }
        }
    }

    public string GetDatabasePath()
    {
        return Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }

    public bool DatabaseFileExists()
    {
        var path = GetDatabasePath();
        return File.Exists(path);
        }

        private async Task MigrateUserSettingsTableAsync()
        {
            try
            {
                // Try to create the table if it doesn't exist, or update schema if it does (additive)
                await Connection.CreateTableAsync<UserSettings>();
                
                // Get current columns to verify migration
                var columns = await Connection.GetTableInfoAsync("UserSettings");
                var columnNames = columns.Select(c => c.Name).ToList();
                
                Debug.WriteLine($"UserSettings table schema check. Columns: {string.Join(", ", columnNames)}");

                // Check for specific columns and add them if missing (as a fallback if CreateTableAsync didn't add them)
                if (!columnNames.Contains("IsProtected"))
                {
                    Debug.WriteLine("Adding missing IsProtected column to UserSettings");
                    await Connection.ExecuteAsync("ALTER TABLE UserSettings ADD COLUMN IsProtected INTEGER NOT NULL DEFAULT 0");
                }
                
                if (!columnNames.Contains("PIN"))
                {
                    Debug.WriteLine("Adding missing PIN column to UserSettings");
                    await Connection.ExecuteAsync("ALTER TABLE UserSettings ADD COLUMN PIN TEXT DEFAULT ''");
                }

                if (!columnNames.Contains("Theme"))
                {
                    Debug.WriteLine("Adding missing Theme column to UserSettings");
                    await Connection.ExecuteAsync("ALTER TABLE UserSettings ADD COLUMN Theme TEXT NOT NULL DEFAULT 'Light'");
                }

                if (!columnNames.Contains("LastLogin"))
                {
                    Debug.WriteLine("Adding missing LastLogin column to UserSettings");
                    await Connection.ExecuteAsync("ALTER TABLE UserSettings ADD COLUMN LastLogin TEXT");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Migration failed: {ex.Message}");
                throw;
            }
        }

        public async Task<DatabaseStatus> GetDatabaseStatusAsync()
        {
        var status = new DatabaseStatus
        {
            DatabasePath = GetDatabasePath(),
            FileExists = DatabaseFileExists()
        };

        if (!status.FileExists)
        {
            return status;
        }

        try
        {
            // Check table existence and counts
            status.MoodCount = await Connection.Table<Mood>().CountAsync();
            status.UserSettingsCount = await Connection.Table<UserSettings>().CountAsync();
            status.JournalEntryCount = await Connection.Table<JournalEntry>().CountAsync();

            // Check if tables have expected data
            status.HasMoods = status.MoodCount > 0;
            status.HasUserSettings = status.UserSettingsCount > 0;

            status.IsValid = status.HasMoods && status.HasUserSettings;

            if (!status.HasMoods) status.ErrorMessage += " Moods not seeded. ";
            if (!status.HasUserSettings) status.ErrorMessage += " User settings missing. ";

            var fileInfo = new FileInfo(status.DatabasePath);
            status.FileSize = fileInfo.Length;
            status.CreatedDate = fileInfo.CreationTime;
            status.ModifiedDate = fileInfo.LastWriteTime;
        }
        catch (Exception ex)
        {
            status.ErrorMessage += $" Diagnostic error: {ex.Message}";
            status.IsValid = false;
        }

        return status;
    }

    public class DatabaseStatus
    {
        public string DatabasePath { get; set; } = string.Empty;
        public bool FileExists { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int MoodCount { get; set; }
        public int UserSettingsCount { get; set; }
        public int JournalEntryCount { get; set; }
        public bool HasMoods { get; set; }
        public bool HasUserSettings { get; set; }
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