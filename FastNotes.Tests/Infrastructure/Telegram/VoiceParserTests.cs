using FastNotes.Api.Infrastructure;

public class VoiceParserTests
{
    [Fact(DisplayName = "Parse 'today' as the current date")]
    public void Parse_Today_ShouldSetToday()
    {
        var input = "buy milk today";
        var (title, dueDate, assignedTo) = VoiceParser.Parse(input);

        Assert.Equal("buy milk", title);
        Assert.Equal(DateTime.Today, dueDate?.Date);
        Assert.Null(assignedTo);
    }

    [Fact(DisplayName = "Parse 'tomorrow morning' as 09:00")]
    public void Parse_TomorrowMorning()
    {
        var input = "Call tomorrow morning";
        var (title, dueDate, _) = VoiceParser.Parse(input);

        var expected = DateTime.Today.AddDays(1).AddHours(9);

        Assert.Equal("call", title);
        Assert.Equal(expected, dueDate);
    }

    [Fact(DisplayName = "Parse day of week with time")]
    public void Parse_FridayWithTime()
    {
        var input = "Prepare report on Friday at 14:30";
        var (title, dueDate, _) = VoiceParser.Parse(input);

        Assert.Equal("prepare report", title);
        Assert.NotNull(dueDate);
        Assert.Equal(14, dueDate?.Hour);
        Assert.Equal(30, dueDate?.Minute);
    }

    [Fact(DisplayName = "Parse 'in 2 days'")]
    public void Parse_After2Days()
    {
        var input = "Meeting in 2 days";
        var (title, dueDate, _) = VoiceParser.Parse(input);

        var expected = DateTime.Today.AddDays(2);

        Assert.Equal("meeting", title);
        Assert.Equal(expected.Date, dueDate?.Date);
    }

    [Fact(DisplayName = "Parse 'in 3 hours'")]
    public void Parse_After3Hours()
    {
        var input = "Call in 3 hours";
        var (title, dueDate, _) = VoiceParser.Parse(input);

        Assert.Equal("call", title);
        Assert.NotNull(dueDate);
    }

    [Fact(DisplayName = "Parse 'day after tomorrow evening'")]
    public void Parse_DayAfterTomorrowEvening()
    {
        var input = "Order a cake the day after tomorrow evening";
        var (title, dueDate, _) = VoiceParser.Parse(input);

        var expected = DateTime.Today.AddDays(2).AddHours(19);

        Assert.Equal("order a cake", title);
        Assert.Equal(expected, dueDate);
    }

    [Fact(DisplayName = "Parse assignee using dash")]
    public void Parse_AssignedDash()
    {
        var input = "Create presentation — Max";
        var (_, _, assignedTo) = VoiceParser.Parse(input);

        Assert.Equal("Max", assignedTo);
    }

    [Fact(DisplayName = "Parse assignee using 'for'")]
    public void Parse_AssignedFor()
    {
        var input = "Book tickets for Anna";
        var (_, _, assignedTo) = VoiceParser.Parse(input);

        Assert.Equal("Anna", assignedTo);
    }

    [Fact(DisplayName = "Parse assignee using comma")]
    public void Parse_AssignedComma()
    {
        var input = "Book tickets, Anna";
        var (_, _, assignedTo) = VoiceParser.Parse(input);

        Assert.Equal("Anna", assignedTo);
    }

    [Fact(DisplayName = "Parse assignee using explicit keyword")]
    public void Parse_AssignedExplicit()
    {
        var input = "Take out the trash assignee: Alex";
        var (_, _, assignedTo) = VoiceParser.Parse(input);

        Assert.Equal("Alex", assignedTo);
    }

    [Fact(DisplayName = "Parse nearest Tuesday")]
    public void Parse_DayOfWeek()
    {
        var input = "Meeting on Tuesday";
        var (title, dueDate, _) = VoiceParser.Parse(input);

        Assert.Equal("meeting", title);
        Assert.NotNull(dueDate);
        Assert.Equal(DayOfWeek.Tuesday, dueDate?.DayOfWeek);
    }
}
