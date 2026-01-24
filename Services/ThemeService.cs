using MyJournals.Database;
using MyJournals.Models;

namespace MyJournals.Services;

public class ThemeService
{
    private readonly AppDatabase _database;
    
    public ThemeService(AppDatabase database)
    {
        _database = database;
    }
    
    public async Task<string> GetThemeAsync()
    {
        var settings = await _database.Connection.Table<UserSettings>().FirstOrDefaultAsync();
        return settings?.Theme ?? "Light";
    }
    
    public async Task SetThemeAsync(string theme)
    {
        var settings = await _database.Connection.Table<UserSettings>().FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new UserSettings { Theme = theme };
            await _database.Connection.InsertAsync(settings);
        }
        else
        {
            settings.Theme = theme;
            await _database.Connection.UpdateAsync(settings);
        }
    }
}
