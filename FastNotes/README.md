# FastNotes – Telegram voice task manager

FastNotes turns Telegram voice messages into tasks, stores them in a database, and
shows them in a web UI. This project is a portfolio showcase with an end-to-end flow
from Telegram bot to API, database, and frontend, plus containerized setup.

## Key features

- Voice → text → task (Telegram → API → DB → UI).
- Task management through a REST API.
- Web UI for viewing and tracking tasks.
- Docker Compose for quick local startup.

## Architecture (high level)

1. A user sends a voice message in Telegram.
2. The bot downloads audio and sends it to speech recognition (Whisper).
3. The API stores the task in PostgreSQL.
4. The UI fetches tasks via the REST API.

## Tech stack

- .NET 8 (ASP.NET Core Web API)
- PostgreSQL + EF Core
- React + Vite + Tailwind
- Telegram.Bot API v22
- Whisper (speech recognition)

## Quick start (Docker)

```bash
cd docker
docker compose up --build
```

- API: http://localhost:5000
- UI: http://localhost:3000

## Local run (without Docker)

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

## Configuration

Minimum required environment variables:

- `ConnectionStrings__DefaultConnection` — PostgreSQL connection string.
- `Telegram__BotToken` — Telegram bot token (store in secrets).

## Demo flow

1. Send a voice message to the Telegram bot.
2. The bot recognizes text and creates a task.
3. Open the UI and confirm the task appears in the list.

## Portfolio roadmap

- [ ] Full Whisper integration (replace the placeholder with real recognition).
- [ ] Task CRUD (update/delete/confirm).
- [ ] Input validation + consistent API error format.
- [ ] API documentation (OpenAPI + examples).
- [ ] Authentication and authorization (JWT).
- [ ] UI: task creation/editing and filters.
