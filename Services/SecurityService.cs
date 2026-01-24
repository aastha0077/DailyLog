using MyJournals.Database;
using MyJournals.Models;

namespace MyJournals.Services;

public class SecurityService : BaseService
{
    public SecurityService(AppDatabase database) : base(database)
    {
    }
    
    public async Task<UserSettings> GetUserSettingsAsync()
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            if (!await IsDatabaseReadyAsync())
                return new UserSettings { Theme = "Light" };

            var settings = await _database.Connection.Table<UserSettings>().FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new UserSettings { Theme = "Light" };
                await _database.Connection.InsertAsync(settings);
            }
            return settings;
        });
    }
    
    public async Task UpdateUserSettingsAsync(UserSettings settings)
    {
        await ExecuteWithRetryAsync<object>(async () =>
        {
            if (!await IsDatabaseReadyAsync())
                throw new InvalidOperationException("Database not available");
                
            await _database.Connection.UpdateAsync(settings);
            return null!;
        });
    }
    
    public async Task SetThemeAsync(string theme)
    {
        var settings = await GetUserSettingsAsync();
        settings.Theme = theme;
        await UpdateUserSettingsAsync(settings);
    }
    
    public async Task<string> GetCurrentThemeAsync()
    {
        var settings = await GetUserSettingsAsync();
        return settings.Theme ?? "Light";
    }
    public async Task<bool> IsProtectedAsync()
    {
        var settings = await GetUserSettingsAsync();
        return settings.IsProtected;
    }

    public async Task<bool> VerifyPINAsync(string pin)
    {
        var settings = await GetUserSettingsAsync();
        if (!settings.IsProtected)
            return true;
            
        return settings.PIN == pin;
    }

    public async Task SetPINAsync(string pin)
    {
        var settings = await GetUserSettingsAsync();
        settings.PIN = pin;
        settings.IsProtected = true;
        await UpdateUserSettingsAsync(settings);
    }
    
    public async Task DisableProtectionAsync()
    {
        var settings = await GetUserSettingsAsync();
        settings.IsProtected = false;
        settings.PIN = null;
        await UpdateUserSettingsAsync(settings);
    }
}
