#!/usr/bin/env python3
"""
Скрипт для автоматического создания GitHub Project Board для Greenhouse Guardian.
Требования:
1. Установленный GitHub CLI (gh)
2. Авторизация: gh auth login
3. Репозиторий должен быть инициализирован на GitHub

Запуск:
python setup-github-project.py <owner>/<repo>
Пример:
python setup-github-project.py myuser/greenhouse-guardian
"""

import subprocess
import sys
import json
import time

def run_gh_command(args):
    """Выполняет команду gh и возвращает результат."""
    try:
        result = subprocess.run(["gh"] + args, capture_output=True, text=True, check=True)
        return result.stdout.strip()
    except subprocess.CalledProcessError as e:
        print(f"Ошибка выполнения команды: {' '.join(args)}")
        print(f"stderr: {e.stderr}")
        sys.exit(1)

def create_project(repo_name):
    print(f"🚀 Создание проекта для репозитория: {repo_name}")
    
    # 1. Создаем проект (тип: BOARD)
    print("📌 Создание доски проекта...")
    project_id = run_gh_command([
        "project", "create", 
        "--title", "Greenhouse Guardian Development", 
        "--type", "BOARD"
    ])
    print(f"✅ Проект создан. ID: {project_id}")
    
    # 2. Добавляем поля (если нужно, но для начала используем стандартные)
    # Стандартные поля в новых проектах: Title, Status, Assignee, Labels
    
    # 3. Определяем задачи из бэклога
    backlog_items = [
        # Milestone 1
        ("[M1] Setup Solution Structure", "high", "backend"),
        ("[M1] Database Design & EF Core Models", "high", "backend"),
        ("[M1] Docker Infrastructure (Postgres, Redis)", "high", "devops"),
        ("[M1] JWT Authentication & Roles", "high", "security"),
        ("[M1] Repository Pattern Implementation", "medium", "backend"),
        
        # Milestone 2
        ("[M2] Sensor Service & Aggregation", "high", "backend"),
        ("[M2] Actuator Service & Command Logging", "high", "backend"),
        ("[M2] Zone & Plant Profile Service", "medium", "backend"),
        ("[M2] Automation Rule Engine", "critical", "backend"),
        ("[M2] Notification Service (Email, Telegram)", "medium", "integration"),
        ("[M2] Alert System & Thresholds", "high", "backend"),
        
        # Milestone 3
        ("[M3] Device Emulator (100 sensors)", "medium", "tools"),
        ("[M3] SignalR Hub for Real-time Telemetry", "high", "backend"),
        ("[M3] Database Seed Data Script", "low", "database"),
        
        # Milestone 4
        ("[M4] Blazor WASM Project Setup & MudBlazor", "high", "frontend"),
        ("[M4] Authentication Pages (Login/Register)", "high", "frontend"),
        ("[M4] Main Dashboard with Real-time Widgets", "critical", "frontend"),
        ("[M4] Sensors Management Page & Charts", "high", "frontend"),
        ("[M4] Actuators Control Page", "medium", "frontend"),
        ("[M4] Zones & Plants Configuration", "medium", "frontend"),
        ("[M4] Automation Rules Builder UI", "high", "frontend"),
        ("[M4] Audit Log Viewer", "low", "frontend"),
        
        # Milestone 5
        ("[M5] CI/CD Pipeline (GitHub Actions)", "medium", "devops"),
        ("[M5] Production Configuration", "medium", "devops"),
        ("[M5] API Documentation (Swagger) & README", "low", "docs"),
    ]

    print(f"📝 Добавление {len(backlog_items)} задач в бэклог...")
    
    item_ids = []
    for title, priority, label in backlog_items:
        # Создаем задачу (Issue) в репозитории
        issue_cmd = [
            "issue", "create", 
            "--title", title,
            "--label", f"priority:{priority}",
            "--label", f"area:{label}"
        ]
        
        # Попытка создать лейблы, если их нет (игнорируем ошибки, если уже есть)
        subprocess.run(["gh", "label", "create", f"priority:{priority}", "--color", "ff0000" if priority == "critical" else "ffff00" if priority == "high" else "00ff00"], capture_output=True)
        subprocess.run(["gh", "label", "create", f"area:{label}", "--color", "0000ff"], capture_output=True)
        
        issue_id = run_gh_command(issue_cmd)
        # issue_id обычно возвращается как URL или номер, нам нужен номер для привязки
        # gh issue create возвращает URL, извлечем номер
        issue_number = issue_id.split("/")[-1]
        
        # Добавляем задачу в проект
        # Примечание: В новых проектах GitHub API немного отличается, используем project-item-add
        try:
            item_id = run_gh_command([
                "project", "item-add", project_id, 
                "--url", issue_id
            ])
            item_ids.append((issue_number, item_id))
            print(f"   ✅ Добавлено: #{issue_number} {title}")
        except Exception as e:
            print(f"   ⚠️ Не удалось добавить в проект #{issue_number}: {e}")

    print("\n🎉 Проект успешно создан!")
    print(f"🔗 Откройте проект: https://github.com/{repo_name}/projects/1") # Обычно первый проект
    print("\n💡 Совет: Используйте 'gh project' для дальнейшего управления.")

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Использование: python setup-github-project.py <owner>/<repo>")
        print("Пример: python setup-github-project.py myuser/greenhouse-guardian")
        sys.exit(1)

    repo_arg = sys.argv[1]
    
    # Проверка наличия gh
    try:
        subprocess.run(["gh", "--version"], check=True, capture_output=True)
    except FileNotFoundError:
        print("❌ Ошибка: GitHub CLI (gh) не найден.")
        print("Установите его: https://cli.github.com/")
        sys.exit(1)
    
    create_project(repo_arg)
