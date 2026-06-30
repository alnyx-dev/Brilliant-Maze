# Brilliant Maze

Прототип игры от первого лица: лабиринт, бриллианты, враги.

## Управление

| Действие | Клавиша |
|---|---|
| Движение | WASD |
| Обзор камеры | Мышь |
| Бег | Shift (удерживать) |
| Прыжок | Пробел |
| Пауза / разблокировать курсор | Escape |

## Архитектура

### Координатор

- **`GameManager`** — точка входа в игровую логику. Хранит состояние игры (`Playing`, `Won`, `Lost`). Все ключевые события (смерть, сбор бриллианта, выход) приходят через его методы. Не singleton — ссылка через Inspector.

### Движение и камера

- **`FirstPersonController`** — движение игрока (WASD + бег + прыжок), вращение камеры мышью, звуки шагов. Использует `CharacterController` и новый Input System.
- **`PlayerControls`** — автоматически сгенерированная обёртка над Input System (WASD, Look, Jump, Sprint).

### Бриллианты

- **`DiamondSpawner`** — спаунит заданное количество бриллиантов в случайных позициях между точками.
- **`DiamondPickup`** — компонент на игроке. При касании бриллианта (триггер с тегом `Diamond`) — деактивирует его, увеличивает счётчик, уведомляет `GameManager`.
- **`DiamondCounter`** — хранит количество собранных бриллиантов и общее число.
- **`Rotate`** — визуальное вращение бриллианта.

### Враги

- **`EnemyAI`** — патрулирование по waypoints, обнаружение игрока (дальность + угол + raycast на стены), погоня, убийство при контакте. Использует `NavMeshAgent`.

### Состояние игры

- **`WinTrigger`** — триггер зоны выхода. Вызывает `GameManager.ReportExitReached()`.
- **`GameManager`** — проверяет условие победы (все бриллианты собраны). При победе → `GameUI.ShowWin()`. При поражении → `GameUI.ShowDeath()`.

### UI

- **`GameUI`** — управляет панелями: поражение, победа, счётчик бриллиантов, блокировка выхода. Кнопки Restart, MainMenu, Quit.

### Лабиринт

- **`MazeGenerator`** — процедурная генерация лабиринта (алгоритм Краскала). Генерирует стены и пол как procedural mesh. Опциональные комнаты и петли.

## Схема связей

```
GameManager
  ├── DiamondCounter
  ├── GameUI
  └── DiamondSpawner

FirstPersonController ──→ GameManager (ReportDeath)
EnemyAI (x2)            ──→ GameManager (ReportDeath)
DiamondPickup           ──→ GameManager (ReportDiamondCollected)
WinTrigger              ──→ GameManager (ReportExitReached)
```

Все связи — через `[SerializeField]` в Inspector.
