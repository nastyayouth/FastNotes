# Architecture

This document provides a short overview of the architecture and a minimal sequence flow.

## Overview (simple)

- **Telegram Bot (ASP.NET Core)** receives voice notes and drives the draft confirmation flow.
- **API (ASP.NET Core)** handles task management and transcription orchestration.
- **PostgreSQL** stores tasks and metadata.
- **Web UI (React)** displays tasks and filters.
- **External services**: Telegram Bot API and OpenAI Whisper.
## Sequence (happy path)

```mermaid
sequenceDiagram
  participant User
  participant Telegram
  participant Bot as Telegram Bot
  participant Whisper as OpenAI Whisper
  participant Db as PostgreSQL
  participant UI as Web UI

  User->>Telegram: Send voice note
  Telegram->>Bot: Webhook update (voice)
  Bot->>Whisper: Upload audio for transcription
  Whisper-->>Bot: Transcript text
  Bot->>Bot: Parse text into draft
  Bot->>Telegram: Ask user to confirm/edit
  User-->>Telegram: Confirm or edit
  Telegram->>Bot: Confirmation callback
  Bot->>Db: Save task
  UI->>Bot: GET /api/tasks
  Bot-->>UI: Task list JSON
```