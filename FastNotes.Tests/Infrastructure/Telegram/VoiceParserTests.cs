using FastNotes.Api.Infrastructure;


public class VoiceParserTests
{
    [Fact(DisplayName = "Parse 'сегодня' как текущую дату")]
    public void Parse_Segodnya_ShouldSetToday()
    {
        var input = "купить молоко сегодня";
        var (title, dueDate, assignedTo) = VoiceParser.Parse(input);

        Assert.Equal("купить молоко", title);
        Assert.Equal(DateTime.Today, dueDate?.Date);
        Assert.Null(assignedTo);
    }

    [Fact(DisplayName = "Парсинг 'завтра утром' в 09:00")]
    public void Parse_TomorrowMorning()
    {
        var input = "Созвон завтра утром";
        var (title, dueDate, _) = VoiceParser.Parse(input);
        var expected = DateTime.Today.AddDays(1).AddHours(9);
        Assert.Equal("созвон", title);
        Assert.Equal(expected, dueDate);
    }

    [Fact(DisplayName = "Парсинг дня недели и времени")]
    public void Parse_FridayWithTime()
    {
        var input = "Подготовить отчет в пятницу в 14:30";
        var (title, dueDate, _) = VoiceParser.Parse(input);
        Assert.Equal("подготовить отчет", title);
        Assert.NotNull(dueDate);
        Assert.Equal(14, dueDate?.Hour);
        Assert.Equal(30, dueDate?.Minute);
    }

    [Fact(DisplayName = "Парсинг через 2 дня")]
    public void Parse_After2Days()
    {
        var input = "Встреча через 2 дня";
        var (title, dueDate, _) = VoiceParser.Parse(input);
        var expected = DateTime.Today.AddDays(2);
        Assert.Equal("встреча", title);
        Assert.Equal(expected.Date, dueDate?.Date);
    }

    [Fact(DisplayName = "Парсинг через 3 часа")]
    public void Parse_After3Hours()
    {
        var input = "Позвонить через 3 часа";
        var (title, dueDate, _) = VoiceParser.Parse(input);
        Assert.Equal("позвонить", title);
        Assert.NotNull(dueDate);
    }

    [Fact(DisplayName = "Парсинг послезавтра вечером")]
    public void Parse_DayAfterTomorrowEvening()
    {
        var input = "Заказать торт послезавтра вечером";
        var (title, dueDate, _) = VoiceParser.Parse(input);
        var expected = DateTime.Today.AddDays(2).AddHours(19);
        Assert.Equal("заказать торт", title);
        Assert.Equal(expected, dueDate);
    }

    [Fact(DisplayName = "Парсинг исполнителя через тире")]
    public void Parse_AssignedDash()
    {
        var input = "Сделать презентацию — Максим";
        var (_, _, assignedTo) = VoiceParser.Parse(input);
        Assert.Equal("Максим", assignedTo);
    }

    [Fact(DisplayName = "Парсинг исполнителя через 'для' ")]
    public void Parse_AssignedFor()
    {
        var input = "Забронировать билеты для Анны";
        var (_, _, assignedTo) = VoiceParser.Parse(input);
        Assert.Equal("Анна", assignedTo);
    }
    
    [Fact(DisplayName = "Парсинг запятой при обращении")]
    public void Parse_AssignedComma()
    {
        var input = "Забронировать билеты, Анна";
        var (_, _, assignedTo) = VoiceParser.Parse(input);
        Assert.Equal("Анна", assignedTo);
    }

    [Fact(DisplayName = "Парсинг исполнителя через ключевое слово")]
    public void Parse_AssignedExplicit()
    {
        var input = "Вынести мусор исполнитель: Алексей";
        var (_, _, assignedTo) = VoiceParser.Parse(input);
        Assert.Equal("Алексей", assignedTo);
    }

    [Fact(DisplayName = "Парсинг ближайшего вторника")]
    public void Parse_DayOfWeek()
    {
        var input = "Встреча во вторник";
        var (title, dueDate, _) = VoiceParser.Parse(input);
        Assert.Equal("встреча", title);
        Assert.NotNull(dueDate);
        Assert.Equal(DayOfWeek.Tuesday, dueDate?.DayOfWeek);
    }
}
