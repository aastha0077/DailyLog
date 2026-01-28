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
        catch
        {
            return false;
        }
    }
}
