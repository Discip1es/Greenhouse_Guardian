# GitHub Projects Setup для Greenhouse Guardian

Этот файл содержит описание структуры GitHub Projects для управления разработкой проекта **Greenhouse Guardian**.

## 📋 Инструкция по настройке

### Вариант 1: Автоматическая настройка (рекомендуется)

1. Установите [GitHub CLI](https://cli.github.com/)
2. Авторизуйтесь: `gh auth login`
3. Запустите скрипт:
   ```bash
   python setup-github-project.py <ваш-логин>/greenhouse-guardian
   ```

### Вариант 2: Ручная настройка

Если вы предпочитаете настроить проект вручную через веб-интерфейс, следуйте этой структуре:

---

## 🏗 Структура проекта

### Название проекта
**Greenhouse Guardian Development**

### Тип
**Board** (Канбан-доска)

---

## 📊 Столбцы (Status)

| Название | Описание |
|----------|----------|
| 📋 Backlog | Все запланированные задачи |
| 🔍 Ready | Задачи, готовые к работе (уточнены требования) |
| 🚀 In Progress | Активно выполняемые задачи |
| 👀 In Review | Код написан, ожидает ревью |
| ✅ Done | Завершённые задачи |

---

## 🏷 Лейблы (Labels)

### Приоритеты (Priority)
| Лейбл | Цвет | Описание |
|-------|------|----------|
| `priority:critical` | 🔴 `#FF0000` | Критично для MVP |
| `priority:high` | 🟠 `#FFA500` | Высокий приоритет |
| `priority:medium` | 🟡 `#FFFF00` | Средний приоритет |
| `priority:low` | 🟢 `#00FF00` | Низкий приоритет |

### Области (Area)
| Лейбл | Цвет | Описание |
|-------|------|----------|
| `area:backend` | 🔵 `#0000FF` | Backend API, сервисы |
| `area:frontend` | 🟣 `#800080` | Blazor WASM UI |
| `area:database` | 🟤 `#A52A2A` | БД, миграции, seed |
| `area:devops` | ⚫ `#000000` | Docker, CI/CD, инфраструктура |
| `area:security` | 🔒 `#808080` | Аутентификация, авторизация |
| `area:integration` | 🟠 `#FFA500` | Внешние интеграции |
| `area:tools` | 🛠 `#00CED1` | Эмуляторы, утилиты |
| `area:docs` | 📄 `#ADD8E6` | Документация |

### Милстоуны (Milestones)
| Лейбл | Описание |
|-------|----------|
| `milestone:1` | M1: Архитектура и инфраструктура |
| `milestone:2` | M2: Основные сервисы |
| `milestone:3` | M3: Real-time и эмуляция |
| `milestone:4` | M4: Frontend UI |
| `milestone:5` | M5: Релиз и деплой |

---

## 📝 Бэклог задач

### Milestone 1: Архитектура и инфраструктура
- [ ] `[M1] Setup Solution Structure` - Создание структуры решения (.sln), проектов
- [ ] `[M1] Database Design & EF Core Models` - Проектирование БД, создание моделей
- [ ] `[M1] Docker Infrastructure (Postgres, Redis)` - Настройка контейнеров
- [ ] `[M1] JWT Authentication & Roles` - Система аутентификации
- [ ] `[M1] Repository Pattern Implementation` - Паттерн репозиториев

### Milestone 2: Основные сервисы
- [ ] `[M2] Sensor Service & Aggregation` - Сервис датчиков
- [ ] `[M2] Actuator Service & Command Logging` - Сервис исполнительных устройств
- [ ] `[M2] Zone & Plant Profile Service` - Зоны и профили растений
- [ ] `[M2] Automation Rule Engine` - Движок правил автоматизации
- [ ] `[M2] Notification Service (Email, Telegram)` - Сервис уведомлений
- [ ] `[M2] Alert System & Thresholds` - Система алертов

### Milestone 3: Real-time и эмуляция
- [ ] `[M3] Device Emulator (100 sensors)` - Эмулятор устройств
- [ ] `[M3] SignalR Hub for Real-time Telemetry` - SignalR хаб
- [ ] `[M3] Database Seed Data Script` - Seed данные для тестирования

### Milestone 4: Frontend UI
- [ ] `[M4] Blazor WASM Project Setup & MudBlazor` - Настройка фронтенда
- [ ] `[M4] Authentication Pages (Login/Register)` - Страницы аутентификации
- [ ] `[M4] Main Dashboard with Real-time Widgets` - Главный дашборд
- [ ] `[M4] Sensors Management Page & Charts` - Управление датчиками
- [ ] `[M4] Actuators Control Page` - Управление актуаторами
- [ ] `[M4] Zones & Plants Configuration` - Настройка зон
- [ ] `[M4] Automation Rules Builder UI` - Конструктор правил
- [ ] `[M4] Audit Log Viewer` - Просмотр аудита

### Milestone 5: Релиз и деплой
- [ ] `[M5] CI/CD Pipeline (GitHub Actions)` - Пайплайны сборки
- [ ] `[M5] Production Configuration` - Продакшн конфигурация
- [ ] `[M5] API Documentation (Swagger) & README` - Документация

---

## 🔧 Поля проекта (Custom Fields)

| Поле | Тип | Опции |
|------|-----|-------|
| Story Points | Number | 1, 2, 3, 5, 8, 13 |
| Sprint | Single Select | Sprint 1, Sprint 2, Sprint 3... |
| Assignee | People | Участники команды |
| Due Date | Date | Дата дедлайна |

---

## 📈 Автоматизация (GitHub Actions Workflows)

Создайте файл `.github/workflows/project-automation.yml`:

```yaml
name: Project Automation

on:
  issues:
    types: [opened, labeled, closed]
  pull_request:
    types: [opened, closed, reopened]

jobs:
  automate-project:
    runs-on: ubuntu-latest
    steps:
      - name: Move to In Review on PR
        if: github.event_name == 'pull_request' && github.event.action == 'opened'
        uses: alex-page/github-project-automation-plus@v0.8.3
        with:
          project: Greenhouse Guardian Development
          column: In Review
          repo-token: ${{ secrets.GITHUB_TOKEN }}
      
      - name: Move to Done on Close
        if: github.event.action == 'closed'
        uses: alex-page/github-project-automation-plus@v0.8.3
        with:
          project: Greenhouse Guardian Development
          column: Done
          repo-token: ${{ secrets.GITHUB_TOKEN }}
```

---

## 🎯 Использование

1. **Новая задача**: Создайте Issue с соответствующими лейблами
2. **Начало работы**: Переместите задачу в "In Progress", назначьте исполнителя
3. **Ревью кода**: При создании PR задача автоматически перейдёт в "In Review"
4. **Завершение**: Закройте PR/Issue - задача перейдёт в "Done"

## 📊 Отчётность

Используйте встроенные View в GitHub Projects:
- **Board View** - Канбан доска
- **Table View** - Таблица всех задач
- **Timeline View** - Диаграмма Ганта

---

## 🔗 Полезные ссылки

- [GitHub Projects Docs](https://docs.github.com/en/issues/planning-and-tracking-with-projects)
- [GitHub CLI Manual](https://cli.github.com/manual/)
- [Project Automation Actions](https://github.com/marketplace/actions/github-project-automation)
