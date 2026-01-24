namespace MyJournals.Models;

public class MoodDistribution
{
    public MoodCategory Category { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class TagUsage
{
    public string Tag { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class StreakInfo
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public List<DateTime> MissedDays { get; set; } = new();
}

public class WordCountTrend
{
    public DateTime Date { get; set; }
    public int WordCount { get; set; }
    public double AverageWordCount { get; set; }
}
