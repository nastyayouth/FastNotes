# FastNotes – Telegram-бот для управления задачами голосом

FastNotes — showcase-проект для демонстрации возможностей .NET и интеграции с Telegram. Бот позволяет быстро создавать и управлять задачами с помощью голосовых команд.

## Цели проекта
- Упростить фиксацию задач через голос в мессенджере.
- Показать архитектуру типичного .NET backend-приложения.

## Архитектура
- **FastNotes.Api** – ASP.NET Core Web API, взаимодействует с Telegram и базой данных.
- **FastNotes.Shared** – общая библиотека с DTO и интерфейсами.
- **fastnotes-ui** – фронтенд на React/Vite/Tailwind.
- **PostgreSQL** – хранилище данных.
- **Whisper** – сервис распознавания речи.

## Технологии
- .NET 8 (ASP.NET Core Web API)
- PostgreSQL + EF Core
- React + Vite + Tailwind
- Telegram.Bot API v22
- Whisper (распознавание речи)

## Репозитории
- Backend: [FastNotes.Api](FastNotes.Api/)
- Shared-библиотека: [FastNotes.Shared](FastNotes.Shared/)
- Frontend: [fastnotes-ui](fastnotes-ui/)

## Запуск через Docker
```bash
cd docker
docker compose up --build
```
API будет доступен на [http://localhost:5000](http://localhost:5000), фронтенд – на [http://localhost:3000](http://localhost:3000), база данных – на порту `5432`.

## Локальная разработка
### Backend
```bash
cd FastNotes.Api
dotnet restore
dotnet run
```

### Frontend
```bash
cd fastnotes-ui
npm install
npm run dev
```

### База данных (PostgreSQL)
```bash
docker run --name fastnotes-db -p 5432:5432 -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=fastnotesdb postgres:15
```

## Roadmap
- Авторизация и учет пользователей.
- Уведомления и напоминания.
- Более мощная панель управления задачами.
- Тесты и CI/CD.
- Скриншоты и GIF с демонстрацией.
