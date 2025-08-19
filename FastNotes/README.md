# FastNotes – Telegram-бот для управления задачами голосом

Проект создан как showcase для .NET backend-разработки.

## Технологии

- .NET 8 (ASP.NET Core Web API)
- PostgreSQL + EF Core
- React + Vite + Tailwind
- Telegram.Bot API v22
- Whisper (распознавание речи)

## Конфигурация

Токен телеграм-бота следует передавать через `appsettings.Development.json` или переменные среды.
Пример секции конфигурации:

```json
{
  "TelegramSettings": {
    "BotToken": "<токен>"
  }
}
```

Для переменной среды используйте ключ `TelegramSettings__BotToken`.
