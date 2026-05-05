# Labels для GitHub проекта Greenhouse Guardian

Этот файл содержит список всех лейблов, которые необходимо создать в репозитории.

## Быстрое создание через CLI

```bash
# Приоритеты
gh label create "priority:critical" --color "FF0000" --description "Критично для MVP"
gh label create "priority:high" --color "FFA500" --description "Высокий приоритет"
gh label create "priority:medium" --color "FFFF00" --description "Средний приоритет"
gh label create "priority:low" --color "00FF00" --description "Низкий приоритет"

# Области (Area)
gh label create "area:backend" --color "0000FF" --description "Backend API, сервисы"
gh label create "area:frontend" --color "800080" --description "Blazor WASM UI"
gh label create "area:database" --color "A52A2A" --description "БД, миграции, seed"
gh label create "area:devops" --color "000000" --description "Docker, CI/CD, инфраструктура"
gh label create "area:security" --color "808080" --description "Аутентификация, авторизация"
gh label create "area:integration" --color "FFA500" --description "Внешние интеграции"
gh label create "area:tools" --color "00CED1" --description "Эмуляторы, утилиты"
gh label create "area:docs" --color "ADD8E6" --description "Документация"

# Статусы и типы
gh label create "bug" --color "D93F0B" --description "Ошибка"
gh label create "enhancement" --color "A2EEEF" --description "Улучшение"
gh label create "development" --color "0E8A16" --description "Задача разработки"
gh label create "question" --color "D876E3" --description "Вопрос"
gh label create "help wanted" --color "008672" --description "Требуется помощь"
gh label create "good first issue" --color "7057FF" --description "Хорошо для новичков"

# Милстоуны (опционально, можно использовать GitHub Milestones)
gh label create "milestone:1" --color "1D76DB" --description "M1: Архитектура и инфраструктура"
gh label create "milestone:2" --color "1D76DB" --description "M2: Основные сервисы"
gh label create "milestone:3" --color "1D76DB" --description "M3: Real-time и эмуляция"
gh label create "milestone:4" --color "1D76DB" --description "M4: Frontend UI"
gh label create "milestone:5" --color "1D76DB" --description "M5: Релиз и деплой"
```

## Таблица всех лейблов

| Название | Цвет | Описание |
|----------|------|----------|
| **Приоритеты** |||
| `priority:critical` | `#FF0000` | 🔴 Критично для MVP |
| `priority:high` | `#FFA500` | 🟠 Высокий приоритет |
| `priority:medium` | `#FFFF00` | 🟡 Средний приоритет |
| `priority:low` | `#00FF00` | 🟢 Низкий приоритет |
| **Области** |||
| `area:backend` | `#0000FF` | Backend API, сервисы |
| `area:frontend` | `#800080` | Blazor WASM UI |
| `area:database` | `#A52A2A` | БД, миграции, seed |
| `area:devops` | `#000000` | Docker, CI/CD, инфраструктура |
| `area:security` | `#808080` | Аутентификация, авторизация |
| `area:integration` | `#FFA500` | Внешние интеграции |
| `area:tools` | `#00CED1` | Эмуляторы, утилиты |
| `area:docs` | `#ADD8E6` | Документация |
| **Типы** |||
| `bug` | `#D93F0B` | Ошибка |
| `enhancement` | `#A2EEEF` | Улучшение |
| `development` | `#0E8A16` | Задача разработки |
| `question` | `#D876E3` | Вопрос |
| `help wanted` | `#008672` | Требуется помощь |
| `good first issue` | `#7057FF` | Хорошо для новичков |
| **Милстоуны** |||
| `milestone:1` | `#1D76DB` | M1: Архитектура и инфраструктура |
| `milestone:2` | `#1D76DB` | M2: Основные сервисы |
| `milestone:3` | `#1D76DB` | M3: Real-time и эмуляция |
| `milestone:4` | `#1D76DB` | M4: Frontend UI |
| `milestone:5` | `#1D76DB` | M5: Релиз и деплой |

## Импорт через CSV

Альтернативно, можно создать файл `labels.csv` и импортировать через настройки репозитория:

```csv
priority:critical,FF0000,Критично для MVP
priority:high,FFA500,Высокий приоритет
priority:medium,FFFF00,Средний приоритет
priority:low,00FF00,Низкий приоритет
area:backend,0000FF,"Backend API, сервисы"
area:frontend,800080,Blazor WASM UI
area:database,A52A2A,"БД, миграции, seed"
area:devops,000000,"Docker, CI/CD, инфраструктура"
area:security,808080,"Аутентификация, авторизация"
area:integration,FFA500,Внешние интеграции
area:tools,00CED1,"Эмуляторы, утилиты"
area:docs,ADD8E6,Документация
bug,D93F0B,Ошибка
enhancement,A2EEEF,Улучшение
development,0E8A16,Задача разработки
question,D876E3,Вопрос
help wanted,008672,Требуется помощь
good first issue,7057FF,Хорошо для новичков
```
