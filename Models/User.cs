using SQLite;

namespace MyJournals.Models;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Unique]
    public string Username { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    
    public string? DisplayName { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastLogin { get; set; }
}
