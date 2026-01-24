using MyJournals.Database;
using MyJournals.Models;

namespace MyJournals.Services;

public abstract class BaseService
{
    protected readonly AppDatabase _database;

    protected BaseService(AppDatabase database)
    {
        _database = database;
    }

    protected async Task<bool> IsDatabaseReadyAsync()
    {
        try
        {
            await _database.InitializeAsync();
            return _database.DatabaseFileExists();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database check failed: {ex.Message}");
            return false;
        }
    }

    protected void LogError(string operation, Exception ex)
    {
        Console.WriteLine($"Error in {operation}: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }

    protected void LogInfo(string message)
    {
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    protected async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                LogError($"Attempt {i + 1} failed", ex);
                await Task.Delay(1000 * (i + 1)); // Exponential backoff
            }
        }
        
        throw new InvalidOperationException($"Operation failed after {maxRetries} attempts");
    }
}
