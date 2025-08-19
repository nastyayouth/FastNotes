# FastNotes UI

## Настройка базового URL

Приложение использует `axios` для обращения к API. Базовый URL берётся из переменной окружения `VITE_API_BASE_URL`.

Создайте файл `.env` в каталоге `fastnotes-ui` и укажите адрес API:

```bash
VITE_API_BASE_URL=http://localhost:5042/api
```

После этого перезапустите дев‑сервер.

## Пример обработки ошибок

```ts
import { getTasks } from './api/tasks';

try {
  const tasks = await getTasks();
  // обработка данных
} catch (err) {
  console.error('Не удалось получить задачи', err);
}
```

Компонент `TaskList` в репозитории демонстрирует использование состояния `error` для отображения сообщения пользователю.
