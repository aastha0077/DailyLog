using MyJournals.Models;

namespace MyJournals.Utils;

public static class MoodHelpers
{
    private static readonly Dictionary<MoodCategory, string> CategoryIcons = new()
    {
        { MoodCategory.Positive, "emoji-smile" },
        { MoodCategory.Neutral, "emoji-neutral" },
        { MoodCategory.Negative, "emoji-frown" }
    };

    private static readonly Dictionary<MoodCategory, string> CategoryColors = new()
    {
        { MoodCategory.Positive, "success" },
        { MoodCategory.Neutral, "warning" },
        { MoodCategory.Negative, "danger" }
    };

    public static string GetCategoryIcon(MoodCategory category)
    {
        return CategoryIcons.TryGetValue(category, out var icon) ? icon : "emoji-neutral";
    }

    public static string GetCategoryColor(MoodCategory category)
    {
        return CategoryColors.TryGetValue(category, out var color) ? color : "secondary";
    }

    public static string GetMoodDisplay(Mood mood)
    {
        return mood?.DisplayName ?? "Unknown";
    }

    public static List<Mood> GetMoodsByCategory(List<Mood> allMoods, MoodCategory category)
    {
        return allMoods.Where(m => m.Category == category).OrderBy(m => m.MoodType).ToList();
    }

    public static Dictionary<MoodCategory, List<Mood>> GroupMoodsByCategory(List<Mood> moods)
    {
        return moods.GroupBy(m => m.Category)
                   .ToDictionary(g => g.Key, g => g.OrderBy(m => m.MoodType).ToList());
    }

    public static string GetMoodEmoji(MoodType moodType)
    {
        return moodType switch
        {
            MoodType.Happy => "😊",
            MoodType.Excited => "🎉",
            MoodType.Relaxed => "😌",
            MoodType.Grateful => "🙏",
            MoodType.Confident => "💪",
            MoodType.Calm => "😌",
            MoodType.Thoughtful => "🤔",
            MoodType.Curious => "🧐",
            MoodType.Nostalgic => "📷",
            MoodType.Bored => "😐",
            MoodType.Sad => "😢",
            MoodType.Angry => "😠",
            MoodType.Stressed => "😰",
            MoodType.Lonely => "😔",
            MoodType.Anxious => "😟",
            _ => "😐"
        };
    }
}
