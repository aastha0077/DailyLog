using SQLite;

namespace MyJournals.Models;

public enum MoodCategory
{
    Positive,
    Neutral,
    Negative
}

public enum MoodType
{
    Happy,
    Excited,
    Relaxed,
    Grateful,
    Confident,
    Calm,
    Thoughtful,
    Curious,
    Nostalgic,
    Bored,
    Sad,
    Angry,
    Stressed,
    Lonely,
    Anxious
}

[Table("Moods")]
public class Mood
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [NotNull]
    public MoodType MoodType { get; set; }
    
    [NotNull]
    public MoodCategory Category { get; set; }
    
    public string DisplayName => MoodType.ToString();
}
