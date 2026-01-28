using SQLite;

namespace MyJournals.Models;

[Table("JournalEntries")]
public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [NotNull, Unique]
    public DateTime EntryDate { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    [NotNull]
    public string Content { get; set; } = string.Empty;
    
    public bool IsMarkdown { get; set; } = false;
    
    [NotNull]
    public DateTime CreatedAt { get; set; }
    
    [NotNull]
    public DateTime UpdatedAt { get; set; }
    
    [Indexed]
    public int PrimaryMoodId { get; set; }
    
    [Indexed]
    public int? SecondaryMood1Id { get; set; }
    
    [Indexed]
    public int? SecondaryMood2Id { get; set; }
    
    public string Category { get; set; } = string.Empty;
    
    public string Tags { get; set; } = string.Empty;
    
    public int WordCount { get; set; }
}
