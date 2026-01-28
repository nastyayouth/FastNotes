using System.Globalization;
using System.Text.RegularExpressions;

namespace FastNotes.Api.Infrastructure;

public static class VoiceParser
{
    private static readonly string[] DaysOfWeekEn =
    {
        "on monday", "on tuesday", "on wednesday",
        "on thursday", "on friday", "on saturday", "on sunday"
    };

    public static (string title, DateTime? dueDate, string? assignedTo) Parse(string input)
    {
        var lowered = input.ToLower();
        DateTime now = DateTime.UtcNow;
        DateTime? dueDate = null;
        string? assignedTo = null;

        // Keywords: today / tomorrow / day after tomorrow
        if (lowered.Contains("day after tomorrow"))
            dueDate = DateTime.UtcNow.Date.AddDays(2);
        else if (lowered.Contains("tomorrow"))
            dueDate = DateTime.UtcNow.Date.AddDays(1);
        if (lowered.Contains("today"))
            dueDate = DateTime.UtcNow.Date;

        // Days of week
        foreach (var dayPhrase in DaysOfWeekEn)
        {
            if (lowered.Contains(dayPhrase))
            {
                var dayOfWeek = ParseDayOfWeek(dayPhrase);
                if (dayOfWeek != null)
                    dueDate = NextWeekday(dayOfWeek.Value);
                break;
            }
        }

        // Time (at 14:30)
        var timeMatch = Regex.Match(lowered, @"at\s?(\d{1,2})([:.](\d{2}))?");
        if (timeMatch.Success && dueDate != null)
        {
            int hour = int.Parse(timeMatch.Groups[1].Value);
            int minute = timeMatch.Groups[3].Success ? int.Parse(timeMatch.Groups[3].Value) : 0;
            dueDate = dueDate.Value.Date.AddHours(hour).AddMinutes(minute);
        }

        // In N days / hours
        var delayMatch = Regex.Match(lowered, @"in\s(\d+)\s(day|days|hour|hours)");
        if (delayMatch.Success)
        {
            int number = int.Parse(delayMatch.Groups[1].Value);
            if (delayMatch.Groups[2].Value.StartsWith("day"))
                dueDate = now.AddDays(number);
            else if (delayMatch.Groups[2].Value.StartsWith("hour"))
                dueDate = now.AddHours(number);
        }

        // Morning / evening
        if (lowered.Contains("morning") && dueDate.HasValue)
            dueDate = dueDate.Value.Date.AddHours(9);
        else if (lowered.Contains("evening") && dueDate.HasValue)
            dueDate = dueDate.Value.Date.AddHours(19);

        // Title — everything before trigger word
        var triggerWords = new[]
        {
            "today", "tomorrow", "day after tomorrow",
            "on ", "at ", "in ", "morning", "evening"
        };
        int cutIndex = triggerWords
            .Select(word => lowered.IndexOf(word))
            .Where(i => i > 0)
            .DefaultIfEmpty(lowered.Length)
            .Min();

        var title = lowered[..cutIndex].Trim();
        if (string.IsNullOrWhiteSpace(title))
            title = string.Join(" ", lowered.Split(" ").Take(3));


        // 1. "for Anna"
        var matchFor = Regex.Match(lowered, @"for\s([a-zA-Z]+)");
        if (matchFor.Success)
            assignedTo = Capitalize(matchFor.Groups[1].Value);

        // 2. Dash or comma
        var matchDash = Regex.Match(input, @"[-—–]\s*([a-zA-Z]+)$");
        if (matchDash.Success)
            assignedTo = Capitalize(matchDash.Groups[1].Value);

        var matchComma = Regex.Match(input, @",\s*([a-zA-Z]+)$");
        if (matchComma.Success)
            assignedTo = Capitalize(matchComma.Groups[1].Value);

        // 3. Explicit keyword
        var matchExplicit = Regex.Match(
            lowered,
            @"(assignee|responsible)\s*[:\-]?\s*([a-zA-Z]+)",
            RegexOptions.IgnoreCase
        );
        if (matchExplicit.Success)
            assignedTo = Capitalize(matchExplicit.Groups[2].Value);

        return (title, dueDate, assignedTo);
    }

    private static DayOfWeek? ParseDayOfWeek(string phrase)
    {
        return phrase switch
        {
            "on monday" => DayOfWeek.Monday,
            "on tuesday" => DayOfWeek.Tuesday,
            "on wednesday" => DayOfWeek.Wednesday,
            "on thursday" => DayOfWeek.Thursday,
            "on friday" => DayOfWeek.Friday,
            "on saturday" => DayOfWeek.Saturday,
            "on sunday" => DayOfWeek.Sunday,
            _ => null
        };
    }

    private static DateTime NextWeekday(DayOfWeek day)
    {
        var date = DateTime.UtcNow.Date;
        while (date.DayOfWeek != day)
            date = date.AddDays(1);
        return date;
    }

    private static string Capitalize(string value)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim());
    }
};