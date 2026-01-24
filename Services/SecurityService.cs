using MyJournals.Database;
using MyJournals.Models;
using System.Security.Cryptography;
using System.Text;

namespace MyJournals.Services;

public class SecurityService
{
    private readonly AppDatabase _database;
    
    public SecurityService(AppDatabase database)
    {
        _database = database;
    }
    
    public async Task<UserSettings> GetUserSettingsAsync()
    {
        try
        {
            var settings = await _database.Connection.Table<UserSettings>().FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new UserSettings
                {
                    IsProtected = false,
                    Theme = "Light"
                };
                await _database.Connection.InsertAsync(settings);
            }
            return settings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user settings: {ex.Message}");
            // Return default settings if database fails
            return new UserSettings
            {
                IsProtected = false,
                Theme = "Light"
            };
        }
    }
    
    public async Task UpdateUserSettingsAsync(UserSettings settings)
    {
        await _database.Connection.UpdateAsync(settings);
    }
    
    public async Task SetPINAsync(string pin)
    {
        var settings = await GetUserSettingsAsync();
        settings.PIN = HashPassword(pin);
        settings.IsProtected = true;
        await UpdateUserSettingsAsync(settings);
    }

    public async Task<bool> VerifyPINAsync(string pin)
    {
        var settings = await GetUserSettingsAsync();
        if (!settings.IsProtected || string.IsNullOrEmpty(settings.PIN))
            return true; // No PIN set, allow access

        var hash = HashPassword(pin);
        return hash == settings.PIN;
    }
    
    public async Task<bool> IsProtectedAsync()
    {
        try
        {
            var settings = await GetUserSettingsAsync();
            return settings.IsProtected;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking protection status: {ex.Message}");
            // If database isn't ready yet, assume not protected
            return false;
        }
    }
    
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    public async Task DisableProtectionAsync()
    {
        var settings = await GetUserSettingsAsync();
        settings.IsProtected = false;
        settings.PIN = string.Empty;
        await UpdateUserSettingsAsync(settings);
    }
}
