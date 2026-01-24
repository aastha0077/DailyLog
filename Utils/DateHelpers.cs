namespace MyJournals.Utils;

public static class DateHelpers
{
    public static string ToRelativeString(DateTime date)
    {
        var now = DateTime.Now;
        var span = now - date;

        if (span.TotalDays < 1)
        {
            if (span.TotalHours < 1)
            {
                return span.TotalMinutes < 1 ? "Just now" : $"{(int)span.TotalMinutes} minutes ago";
            }
            return $"{(int)span.TotalHours} hours ago";
        }
        
        if (span.TotalDays < 7)
        {
            return span.TotalDays == 1 ? "Yesterday" : $"{(int)span.TotalDays} days ago";
        }
        
        if (span.TotalDays < 30)
        {
            var weeks = (int)(span.TotalDays / 7);
            return weeks == 1 ? "Last week" : $"{weeks} weeks ago";
        }
        
        if (span.TotalDays < 365)
        {
            var months = (int)(span.TotalDays / 30);
            return months == 1 ? "Last month" : $"{months} months ago";
        }
        
        var years = (int)(span.TotalDays / 365);
        return years == 1 ? "Last year" : $"{years} years ago";
    }

    public static string ToDisplayString(DateTime date, bool includeTime = false)
    {
        if (includeTime)
            return date.ToString("MMM dd, yyyy 'at' h:mm tt");
        
        return date.ToString("MMM dd, yyyy");
    }

    public static List<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
    {
        var dates = new List<DateTime>();
        var currentDate = startDate.Date;
        
        while (currentDate <= endDate.Date)
        {
            dates.Add(currentDate);
            currentDate = currentDate.AddDays(1);
        }
        
        return dates;
    }

    public static bool IsSameDay(DateTime date1, DateTime date2)
    {
        return date1.Date == date2.Date;
    }
}
