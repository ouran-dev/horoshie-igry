# Хорошие игры — документация проекта

Документ для продолжения разработки на другом компьютере.  
Дата актуализации: июнь 2026.

---

## Что это за проект

**«Хорошие игры»** — WPF-приложение (.NET 8) для Windows: каталог мини-игр для детей на сенсорной панели.

- Язык интерфейса: **русский**
- Режим работы: **полноэкранный киоск** (без рамки окна, без выхода по F11/Esc)
- Единый визуальный стиль: библиотеки **Kenney UI** и **Kenney Background Elements**

---

## Требования на новом компьютере

1. **Windows 10/11**
2. **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (не только Runtime)
3. Редактор: Visual Studio 2022, Rider или VS Code + C# Dev Kit
4. Скопировать **всю папку проекта** целиком, включая `Assets/`

Проверка установки:

```bat
dotnet --version
```

Должна быть версия 8.x.

---

## Быстрый запуск

### Способ 1 — двойной клик

```
Запуск.bat
```

Файл лежит в корне проекта и запускает `dotnet run`.

### Способ 2 — из терминала

```bat
cd "путь\к\Хорошие Игры"
dotnet run
```

### Сборка Release

```bat
dotnet build -c Release
```

Готовый exe:

```
bin\Release\net8.0-windows\HoroshieIgry.exe
```

> Если сборка Release не обновляет exe — закройте запущенное приложение и повторите сборку.

---

## Структура папок

```
Хорошие Игры/
├── Запуск.bat                    ← единственный файл запуска
├── HoroshieIgry.sln
├── HoroshieIgry.csproj
├── PROJECT.md                    ← этот файл
├── App.xaml / App.xaml.cs
├── MainWindow.xaml               ← оболочка: шапка + навигация + фон
│
├── Assets/
│   ├── UI/Kenney/                ← SVG-элементы интерфейса (~437 файлов)
│   └── Background Elements/Kenney/
│       └── Vector/
│           └── vector_backgrounds.svg   ← спрайтшит фонов 512×512
│
├── Core/
│   ├── Games/                    ← IGameModule, каталог, регистрация
│   ├── Navigation/               ← NavigationService
│   └── UI/                       ← KenneyPaths, KenneySvg, фоны
│
├── Controls/
│   ├── GameBackground.xaml       ← фоновый слой
│   └── Kenney/                   ← KenneyButton, KenneyPanel, KenneyImage
│
├── Themes/
│   ├── AppResources.xaml
│   └── KenneyResources.xaml      ← стили текста
│
├── Views/
│   ├── MainMenuView              ← каталог игр
│   ├── SettingsView              ← заглушка настроек
│   └── ComingSoonGameView        ← заглушка «скоро»
│
└── Games/
    ├── MemoryGame/               ← ✅ готова
    ├── FindColorGame/            ← заглушка
    ├── FindOddGame/              ← заглушка
    └── PuzzleGame/               ← заглушка
```

---

## Архитектура

```
MainWindow
├── GameBackground          ← нижний слой, тема меняется при навигации
└── Grid
    ├── Шапка (Kenney-кнопки «домой» и «настройки»)
    └── ContentHost         ← сюда подставляются экраны
```

### Навигация

`NavigationService` переключает `ContentHost.Content`:

| Метод | Экран | Фон |
|-------|-------|-----|
| `NavigateToCatalog()` | `MainMenuView` | `BackgroundTheme.Catalog` |
| `NavigateToSettings()` | `SettingsView` | `BackgroundTheme.Catalog` |
| `NavigateToGame(game)` | `game.CreateView()` | `game.BackgroundTheme` |

События:

- `CurrentTitleChanged` — заголовок в шапке
- `BackgroundThemeChanged` — смена фона в `GameBackground`

### Регистрация игр

Единственное место — `Core/Games/GameCatalogRegistrar.cs`:

```csharp
catalog.Register(new MemoryGameModule());
catalog.Register(new FindColorGameModule());
// ...
```

При старте `App.xaml.cs` создаёт `GameCatalog` и вызывает `RegisterAll`.

---

## Контракт мини-игры (`IGameModule`)

```csharp
public interface IGameModule
{
    string Id { get; }              // латиница, без пробелов
    string Title { get; }
    string Description { get; }
    string IconEmoji { get; }       // emoji на карточке каталога
    bool IsAvailable { get; }        // false → кнопка «Скоро», серая
    BackgroundTheme BackgroundTheme { get; }
    UserControl CreateView(INavigationContext navigation);
}
```

### Как добавить новую игру

1. Создать папку `Games/ИмяИгры/`
2. Реализовать `IGameModule` (файл `ИмяGameModule.cs`)
3. Создать `ИмяGameView.xaml` + code-behind с логикой
4. Добавить **одну строку** в `GameCatalogRegistrar.cs`
5. Выбрать `BackgroundTheme` и UI только из Kenney

Шаблон модуля:

```csharp
public sealed class MyGameModule : IGameModule
{
    public string Id => "my-game";
    public string Title => "Моя игра";
    public string Description => "Краткое описание.";
    public string IconEmoji => "🎮";
    public bool IsAvailable => true;
    public BackgroundTheme BackgroundTheme => BackgroundTheme.Meadow;

    public UserControl CreateView(INavigationContext navigation)
        => new MyGameView(navigation);
}
```

Возврат в каталог из игры:

```csharp
_navigation.NavigateToCatalog();
```

---

## Состояние игр

| ID | Название | Статус | Фон |
|----|----------|--------|-----|
| `memory` | Память | ✅ Работает | Meadow (зелёная поляна) |
| `find-color` | Найди цвет | ⏳ Скоро | Clouds (временно) |
| `find-odd` | Найди лишнее | ⏳ Скоро | Forest |
| `puzzle` | Собери картинку | ⏳ Скоро | Mountains |

### Игра «Память» — детали

- Старт: поле **3×3** (4 пары + 1 пустая клетка)
- После победы: предложение перейти на поле **(N+1)×(N+1)** до **8×8**
- Карточки: emoji-символы, анимация переворота
- Ассеты карточек: `KenneyPaths.MemoryCardBack` / `MemoryCardFront`
- Файлы: `Games/MemoryGame/MemoryGameView.*`, `Controls/FlipMemoryCard.*`, `Models/CardModel.cs`

---

## Правила интерфейса (Kenney UI)

**Папка ассетов:** `Assets/UI/Kenney/`

### Обязательно

- Использовать **только** элементы из этой папки
- **Не рисовать** кнопки, панели и окна самостоятельно
- Перед созданием UI-элемента — искать готовый ассет в `KenneyPaths.cs` и папке `Assets/UI/Kenney/`

### Допускается

- Изменить размер
- Добавить тень
- Плавная анимация
- Прозрачность
- Лёгкий цветовой оттенок (как на лицевой стороне карточек памяти)

### Готовые WPF-компоненты

| Компонент | Назначение |
|-----------|------------|
| `KenneyButton` | Кнопки (зелёные, синие, серые, круглые) |
| `KenneyPanel` | Панели (`input_rectangle`, `input_square`, `input_outline_rectangle`) |
| `KenneyImage` | Иконки и декор из SVG |

### Ключевые пути (`Core/UI/KenneyPaths.cs`)

| Элемент | Файл |
|---------|------|
| Основная кнопка | `button_rectangle_depth_flat.svg` (Green) |
| Вторичная кнопка | `button_rectangle_depth_flat.svg` (Blue) |
| Неактивная кнопка | `button_rectangle_depth_flat.svg` (Grey) |
| Круглая кнопка | `button_round_depth_flat.svg` (Blue) |
| Панель | `Extra/input_rectangle.svg` |
| Панель-рамка | `Extra/input_outline_rectangle.svg` |
| Стрелка «назад» | `Blue/arrow_basic_w_small.svg` |
| Звезда (статистика) | `Yellow/star.svg` |

### Загрузка SVG

NuGet: **SharpVectors.Wpf 1.8.5**

```csharp
KenneySvg.Load(relativePath);          // с кэшем
KenneySvg.ApplyTo(image, relativePath);
```

Пути относительно `AppContext.BaseDirectory` (папка с exe после сборки).

### Важный нюанс XAML

Внутри `KenneyPanel` **нельзя** использовать `x:Name` для элементов, к которым нужен доступ из родительского View — они попадают в область имён панели. Используйте **привязки данных** (`DataContext`).

У `KenneyButton` отступы задаются через `ContentPadding` (не `Padding` — конфликт с WPF).

---

## Правила фонов (Kenney Background Elements)

**Папка ассетов:** `Assets/Background Elements/Kenney/`

### Обязательно

- **Не создавать** фоны программно (градиенты, фигуры, Canvas)
- **Не использовать** случайные картинки из интернета
- Фон — **самый нижний слой** (`GameBackground` в `MainWindow`)
- Масштаб: `UniformToFill` (пропорции сохраняются, без искажения)
- Каждая игра выбирает тему через `BackgroundTheme` в модуле

### Темы (`Core/UI/BackgroundTheme.cs`)

| Тема | Сцена в спрайтшите | Координаты (×512) | Использование |
|------|-------------------|-------------------|---------------|
| `Catalog` | небо + холмы | (40, 520) | Каталог, настройки |
| `Meadow` | зелёная поляна | (40, 1042) | Память |
| `Sea` | море | (562, 1042) | — |
| `Desert` | пустыня | (1084, 1042) | — |
| `Forest` | лес | (1084, 520) | Найди лишнее |
| `Mountains` | горы | (40, 0) | Собери картинку |
| `Clouds` | облака | (1084, 0) | Найди цвет (временно) |
| `Space` | космос | (1606, 520) | — |

Спрайтшит: `Vector/vector_backgrounds.svg` — сетка ячеек **512×512**.

Загрузка: `KenneyBackgroundLoader.LoadTheme(theme)` — обрезка + кэш.

### Если подходящего фона нет

Не рисовать новый. Добавить в код модуля:

```csharp
// TODO: Требуется добавить подходящий фон в Assets/Backgrounds
```

Затем добавить сцену в `KenneyBackgroundPaths.cs` — логика игры не меняется.

**Известный TODO:** для «Найди цвет» нужен фон с радугой — пока используется `Clouds`.

### PNG-спрайтшит

В папке есть `Spritesheet/spritesheet_default.xml` (ссылка на PNG), но **PNG-файл в проект не включён**. При добавлении отдельных PNG-фонов — прописать путь в `KenneyBackgroundPaths` и добавить копирование в `.csproj`.

---

## NuGet-зависимости

| Пакет | Версия | Назначение |
|-------|--------|------------|
| SharpVectors.Wpf | 1.8.5 | Рендер SVG (UI и фоны) |

---

## Настройки киоска

`MainWindow.xaml.cs` → `Window_Loaded`:

- `WindowStyle = None`
- `ResizeMode = NoResize`
- `WindowState = Maximized`

Кнопки выхода из полноэкранного режима нет (преднамеренно для сенсорной панели).

---

## Что ещё не сделано (backlog)

- [ ] **Настройки** — сейчас заглушка (`SettingsView`)
- [ ] **Найди цвет**, **Найди лишнее**, **Собери картинку** — заглушки `ComingSoonGameView`
- [ ] Фон с радугой для «Найди цвет»
- [ ] Общие настройки: звук, размер шрифта, профили детей
- [ ] Отдельные PNG-фоны (если появятся в библиотеке Kenney)
- [ ] Git-репозиторий (при необходимости — инициализировать на новом ПК)

---

## Перенос на другой компьютер — чеклист

1. Скопировать папку `Хорошие Игры` целиком (USB, облако, git)
2. Убедиться, что на месте:
   - `Assets/UI/Kenney/` (сотни SVG)
   - `Assets/Background Elements/Kenney/Vector/vector_backgrounds.svg`
   - `HoroshieIgry.sln`, `HoroshieIgry.csproj`, `Запуск.bat`
3. Установить .NET 8 SDK
4. Запустить `Запуск.bat` или `dotnet run`
5. При первом запуске NuGet скачает SharpVectors автоматически

**Не копировать** (создаются заново при сборке):

- `bin/`
- `obj/`

---

## Полезные команды

```bat
dotnet restore
dotnet build
dotnet run
dotnet clean
```

Открыть solution в Visual Studio:

```
HoroshieIgry.sln
```

---

## Лицензии ассетов

В папках Kenney лежат `License.txt` — **Creative Commons CC0** (Kenney.nl).  
Атрибуция не обязательна, но приветствуется.

---

## Контакты / заметки для разработчика

- Целевая аудитория: **дети**, сенсорный экран
- Все мини-игры должны выглядеть как продукт **одной студии**
- Новый UI → сначала `KenneyPaths.cs`, новый фон → `KenneyBackgroundPaths.cs`
- Интерфейс на русском, код и идентификаторы игр — на латинице
