# GitHub Projects для Greenhouse Guardian

## 📋 Обзор

Этот каталог содержит все необходимые файлы для настройки GitHub Projects и автоматизации управления проектом **Greenhouse Guardian**.

## 🚀 Быстрый старт

### Шаг 1: Установка GitHub CLI

```bash
# macOS
brew install gh

# Ubuntu/Debian
sudo apt install gh

# Windows (через winget)
winget install --id GitHub.cli
```

### Шаг 2: Авторизация

```bash
gh auth login
```

Следуйте инструкциям для входа через браузер.

### Шаг 3: Создание лейблов

```bash
cd .github
chmod +x create-labels.sh
./create-labels.sh
```

Или вручную:
```bash
gh label list  # Проверить существующие
gh label create "priority:critical" --color "FF0000"
# ... остальные лейблы из LABELS.md
```

### Шаг 4: Запуск скрипта создания проекта

```bash
# Замените на ваш репозиторий
python setup-github-project.py ваш-логин/greenhouse-guardian
```

### Шаг 5: Настройка Milestones (опционально)

```bash
gh milestone create "M1: Архитектура и инфраструктура"
gh milestone create "M2: Основные сервисы"
gh milestone create "M3: Real-time и эмуляция"
gh milestone create "M4: Frontend UI"
gh milestone create "M5: Релиз и деплой"
```

## 📁 Структура файлов

```
.github/
├── ISSUE_TEMPLATE/
│   ├── bug_report.yml          # Шаблон сообщения об ошибке
│   ├── feature_request.yml     # Шаблон предложения функции
│   └── task.yml                # Шаблон задачи разработки
├── workflows/
│   ├── ci-cd.yml               # CI/CD пайплайн
│   └── project-automation.yml  # Автоматизация проекта
├── LABELS.md                   # Документация по лейблам
└── PULL_REQUEST_TEMPLATE.md    # Шаблон PR (создайте при необходимости)
```

## 🏗 Структура проекта в GitHub

После выполнения скрипта у вас будет:

### Доска проекта
- **Название**: Greenhouse Guardian Development
- **Тип**: Board (Канбан)
- **Столбцы**: Backlog → Ready → In Progress → In Review → Done

### Лейблы
- **Приоритеты**: `priority:critical`, `priority:high`, `priority:medium`, `priority:low`
- **Области**: `area:backend`, `area:frontend`, `area:database`, `area:devops`, etc.
- **Типы**: `bug`, `enhancement`, `development`, `question`

### Автоматизация
- При создании PR → задача перемещается в "In Review"
- При закрытии PR/Issue → задача перемещается в "Done"

## 📊 Бэклог задач

Проект включает 28 задач, разбитых на 5 милстоунов:

### M1: Архитектура и инфраструктура (5 задач)
- Setup Solution Structure
- Database Design & EF Core Models
- Docker Infrastructure
- JWT Authentication & Roles
- Repository Pattern Implementation

### M2: Основные сервисы (6 задач)
- Sensor Service & Aggregation
- Actuator Service & Command Logging
- Zone & Plant Profile Service
- Automation Rule Engine
- Notification Service
- Alert System & Thresholds

### M3: Real-time и эмуляция (3 задачи)
- Device Emulator (100 sensors)
- SignalR Hub for Real-time Telemetry
- Database Seed Data Script

### M4: Frontend UI (8 задач)
- Blazor WASM Project Setup
- Authentication Pages
- Main Dashboard
- Sensors Management Page
- Actuators Control Page
- Zones & Plants Configuration
- Automation Rules Builder UI
- Audit Log Viewer

### M5: Релиз и деплой (3 задачи)
- CI/CD Pipeline
- Production Configuration
- API Documentation & README

## 🔧 Ручная настройка (альтернатива)

Если вы предпочитаете настроить проект вручную через веб-интерфейс:

1. Перейдите в репозиторий на GitHub
2. Вкладка **Projects** → **New project** → **Build a board**
3. Назовите проект "Greenhouse Guardian Development"
4. Добавьте столбцы: Backlog, Ready, In Progress, In Review, Done
5. Создайте лейблы согласно `LABELS.md`
6. Создайте Issues вручную или используйте шаблоны

## 📈 Использование

### Создание новой задачи

```bash
gh issue create --title "[M2] Sensor Service Implementation" \
  --label "priority:high,area:backend,milestone:2" \
  --body "Описание задачи..."
```

Или через веб-интерфейс с использованием шаблонов.

### Просмотр задач

```bash
# Все открытые задачи
gh issue list

# Задачи с высоким приоритетом
gh issue list --label "priority:high"

# Задачи текущего милстоуна
gh issue list --milestone "M2: Основные сервисы"
```

### Перемещение задач

Через веб-интерфейс проекта перетаскивайте карточки между столбцами.

## 🎯 Best Practices

1. **Каждая задача должна иметь**:
   - Приоритет (`priority:*`)
   - Область (`area:*`)
   - Исполнителя (Assignee)
   - Чек-лист требований

2. **Процесс работы**:
   - Новые задачи создаются в Backlog
   - Перед началом работы задача перемещается в Ready (уточнение требований)
   - Начало работы → In Progress (назначить исполнителя)
   - Завершение кода → создать PR (автоматически в In Review)
   - Мерж PR → задача в Done

3. **Ежедневный стендап**:
   - Откройте доску проекта
   - Обсудите задачи в In Progress
   - Выявите блокеры

## 🔗 Полезные ссылки

- [Документация GitHub Projects](https://docs.github.com/en/issues/planning-and-tracking-with-projects)
- [GitHub CLI Manual](https://cli.github.com/manual/)
- [Project Automation Marketplace](https://github.com/marketplace/actions/github-project-automation-plus)

## ❓ Troubleshooting

### Ошибка "project not found"
Убедитесь, что проект создан и у вас есть права доступа.

### Ошибка "label already exists"
Лейбл уже существует, это нормально. Скрипт продолжит работу.

### Проект не отображается в репозитории
Перейдите в Settings → Features → Projects и включите функцию.

---

**Готово!** Теперь у вас есть полноценная система управления проектом в GitHub. 🎉
