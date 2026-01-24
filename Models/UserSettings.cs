using SQLite;

namespace MyJournals.Models;

[Table("UserSettings")]
public class UserSettings
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [NotNull]
    public bool IsProtected { get; set; } = false;

    public string PIN { get; set; } = string.Empty;
    
    [NotNull]
    public string Theme { get; set; } = "Light";
    
    public DateTime LastLogin { get; set; }
}
