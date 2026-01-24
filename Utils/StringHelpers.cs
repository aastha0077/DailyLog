namespace MyJournals.Utils;

public static class StringHelpers
{
    public static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        
        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public static string Truncate(string text, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        
        return text.Substring(0, maxLength - suffix.Length) + suffix;
    }

    public static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;
        
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
    }

    public static List<string> ExtractTags(string tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
            return new List<string>();
        
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static string ConvertTagsToJson(List<string> tags)
    {
        if (tags == null || !tags.Any())
            return "[]";
        
        return System.Text.Json.JsonSerializer.Serialize(tags);
    }
}
