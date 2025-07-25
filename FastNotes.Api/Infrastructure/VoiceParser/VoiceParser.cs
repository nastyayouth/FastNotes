using System.Globalization;
using System.Text.RegularExpressions;

namespace FastNotes.Api.Infrastructure;

public static class VoiceParser
{
    private static readonly string[] DaysOfWeekRu = 
    {
        "в понедельник", "во вторник", "в среду", "в четверг", "в пятницу", "в субботу", "в воскресенье"
    };

    public static (string title, DateTime? dueDate) Parse(string input)
    {
        var lowered = input.ToLower();
        DateTime now = DateTime.Now;
        DateTime? dueDate = null;

        //  Ключевые слова
        if (lowered.Contains("сегодня"))
            dueDate = DateTime.Today;
        else if (lowered.Contains("завтра"))
            dueDate = DateTime.Today.AddDays(1);
        else if (lowered.Contains("послезавтра"))
            dueDate = DateTime.Today.AddDays(2);

        //  Дни недели
        foreach (var dayPhrase in DaysOfWeekRu)
        {
            if (lowered.Contains(dayPhrase))
            {
                var dayOfWeek = ParseDayOfWeek(dayPhrase);
                if (dayOfWeek != null)
                    dueDate = NextWeekday(dayOfWeek.Value);
                break;
            }
        }

        // Время
        var timeMatch = Regex.Match(lowered, @"в\s?(\d{1,2})([:.](\d{2}))?");
        if (timeMatch.Success && dueDate != null)
        {
            int hour = int.Parse(timeMatch.Groups[1].Value);
            int minute = timeMatch.Groups[3].Success ? int.Parse(timeMatch.Groups[3].Value) : 0;
            dueDate = dueDate.Value.Date.AddHours(hour).AddMinutes(minute);
        }

        //  Через N дней/часов
        var delayMatch = Regex.Match(lowered, @"через\s(\d+)\s(д(ень|ня|ней)|час(а|ов)?)");
        if (delayMatch.Success)
        {
            int number = int.Parse(delayMatch.Groups[1].Value);
            if (delayMatch.Groups[2].Value.StartsWith("д"))
                dueDate = now.AddDays(number);
            else if (delayMatch.Groups[2].Value.StartsWith("час"))
                dueDate = now.AddHours(number);
        }

        //  Утром / вечером
        if (lowered.Contains("утром") && dueDate.HasValue)
            dueDate = dueDate.Value.Date.AddHours(9);
        else if (lowered.Contains("вечером") && dueDate.HasValue)
            dueDate = dueDate.Value.Date.AddHours(19);

        // Заголовок — всё до ключевого слова
        var triggerWords = new[] { "сегодня", "завтра", "в ", "во ", "через", "утром", "вечером", "послезавтра" };
        int cutIndex = triggerWords
            .Select(word => lowered.IndexOf(word))
            .Where(i => i > 0)
            .DefaultIfEmpty(lowered.Length)
            .Min();

        var title = lowered[..cutIndex].Trim();
        if (string.IsNullOrWhiteSpace(title))
            title = string.Join(" ", lowered.Split(" ").Take(3));

        return (CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title), dueDate);
    }

    private static DayOfWeek? ParseDayOfWeek(string phrase)
    {
        return phrase switch
        {
            "в понедельник" => DayOfWeek.Monday,
            "во вторник" => DayOfWeek.Tuesday,
            "в среду" => DayOfWeek.Wednesday,
            "в четверг" => DayOfWeek.Thursday,
            "в пятницу" => DayOfWeek.Friday,
            "в субботу" => DayOfWeek.Saturday,
            "в воскресенье" => DayOfWeek.Sunday,
            _ => null
        };
    }

    private static DateTime NextWeekday(DayOfWeek day)
    {
        var date = DateTime.Today;
        while (date.DayOfWeek != day)
            date = date.AddDays(1);
        return date;
    }
};