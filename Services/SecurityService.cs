using MyJournals.Database;
using MyJournals.Models;
using System.Diagnostics;

namespace MyJournals.Services;

public class SecurityService
{
    private readonly AppDatabase _database;
    public bool IsAuthenticated { get; private set; } = false;

    public SecurityService(AppDatabase database)
    {
        _database = database;
    }

    private async Task<bool> IsDatabaseReadyAsync()
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
    public void Authenticate() => IsAuthenticated = true;
    public void Logout() => IsAuthenticated = false;

    public async Task<bool> LoginAsync(string password)
    {
        if (!await IsDatabaseReadyAsync()) return false;

        var settings = await GetUserSettingsAsync();
        
        // If not protected, anyone can log in
        if (!settings.IsProtected) 
        {
            IsAuthenticated = true;
            return true;
        }

        // Compare password (PIN)
        if (settings.PIN == password)
        {
            IsAuthenticated = true;
            settings.LastLogin = DateTime.UtcNow;
            await UpdateUserSettingsAsync(settings);
            return true;
        }

        return false;
    }

    public async Task<bool> IsProtectedAsync()
    {
        if (!await IsDatabaseReadyAsync()) return false;
        var settings = await GetUserSettingsAsync();
        return settings.IsProtected;
    }

    public async Task<bool> VerifyPINAsync(string pin)
    {
        var settings = await GetUserSettingsAsync();
        var sanitizedInput = pin?.Trim() ?? "";
        var storedPin = settings.PIN?.Trim() ?? "";
        
        // Log EVERYTHING to the console
        Console.WriteLine($"[PIN_SYSTEM] VERIFYING...");
        Console.WriteLine($"[PIN_SYSTEM] Stored: '{storedPin}'");
        Console.WriteLine($"[PIN_SYSTEM] Input:  '{sanitizedInput}'");
        Console.WriteLine($"[PIN_SYSTEM] IsProtected: {settings.IsProtected}");
        
        if (!settings.IsProtected) 
        {
            Console.WriteLine("[PIN_SYSTEM] Protection is OFF - bypass login");
            return true;
        }

        if (string.IsNullOrEmpty(sanitizedInput)) 
        {
            Console.WriteLine("[PIN_SYSTEM] Input is EMPTY - fail");
            return false;
        }
        
        bool isMatch = storedPin == sanitizedInput;
        Console.WriteLine($"[PIN_SYSTEM] MATCH RESULT: {isMatch}");
        return isMatch;
    }

    public async Task SetPINAsync(string pin)
    {
        var sanitizedPin = pin?.Trim() ?? "";
        Console.WriteLine($"[PIN_SYSTEM] SETTING NEW PIN: '{sanitizedPin}'");
        
        if (!await IsDatabaseReadyAsync()) 
        {
            Console.WriteLine("[PIN_SYSTEM] DB NOT READY - abort set");
            return;
        }

        var settings = await GetUserSettingsAsync();
        settings.PIN = sanitizedPin;
        settings.IsProtected = true;
        settings.Id = 1; // Force ID 1
        
        await UpdateUserSettingsAsync(settings);
        
        // Final sanity check
        var check = await GetUserSettingsAsync();
        Console.WriteLine($"[PIN_SYSTEM] PIN SAVED. Readback - PIN: '{check.PIN}', Protected: {check.IsProtected}");
    }

    public async Task DisableProtectionAsync()
    {
        if (!await IsDatabaseReadyAsync()) return;
        var settings = await GetUserSettingsAsync();
        settings.IsProtected = false;
        settings.PIN = string.Empty;
        await UpdateUserSettingsAsync(settings);
    }

    public async Task<UserSettings> GetUserSettingsAsync()
    {
        if (!await IsDatabaseReadyAsync()) return new UserSettings { Id = 1, Theme = "Light", IsProtected = false };

        try 
        {
            // Count total rows for diagnostics
            int count = await _database.Connection.Table<UserSettings>().CountAsync();
            Console.WriteLine($"[PIN_SYSTEM] Settings table has {count} rows.");

            // Always try to get row ID 1 first
            var settings = await _database.Connection.Table<UserSettings>().Where(x => x.Id == 1).FirstOrDefaultAsync();
            
            if (settings == null)
            {
                // If ID 1 is missing, try to find ANY existing row
                settings = await _database.Connection.Table<UserSettings>().FirstOrDefaultAsync();
                
                if (settings == null)
                {
                    Console.WriteLine("[PIN_SYSTEM] No settings found at all. Creating NEW row ID 1.");
                    settings = new UserSettings { Id = 1, Theme = "Light", IsProtected = false, PIN = "" };
                    await _database.Connection.InsertAsync(settings);
                }
                else
                {
                    Console.WriteLine($"[PIN_SYSTEM] Found settings with ID {settings.Id}. Migrating to ID 1.");
                    int oldId = settings.Id;
                    settings.Id = 1;
                    // Delete old and replace with ID 1
                    await _database.Connection.DeleteAsync<UserSettings>(oldId);
                    await _database.Connection.InsertOrReplaceAsync(settings);
                }
            }
            return settings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PIN_SYSTEM] ERROR in GetUserSettings: {ex.Message}");
            return new UserSettings { Id = 1, Theme = "Light", IsProtected = false };
        }
    }

    public async Task UpdateUserSettingsAsync(UserSettings settings)
    {
        try 
        {
            settings.Id = 1; // Always force ID 1
            await _database.Connection.InsertOrReplaceAsync(settings);
            Console.WriteLine("[PIN_SYSTEM] Database InsertOrReplace executed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PIN_SYSTEM] DATABASE UPDATE FAILED: {ex.Message}");
            throw;
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        if (!await IsDatabaseReadyAsync()) return;
        var settings = await GetUserSettingsAsync();
        settings.Theme = theme;
        await UpdateUserSettingsAsync(settings);
    }

    public async Task<string> GetCurrentThemeAsync()
    {
        var settings = await GetUserSettingsAsync();
        return settings.Theme ?? "Light";
    }
}
