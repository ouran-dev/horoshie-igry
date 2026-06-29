using System.Windows.Controls;
using HoroshieIgry.Core.Navigation;
using HoroshieIgry.Core.UI;

namespace HoroshieIgry.Core.Games;

/// <summary>
/// Контракт мини-игры в каталоге «Хорошие игры».
/// Создайте класс <c>*GameModule</c> в папке <c>Games/</c> — он появится в каталоге автоматически.
/// </summary>
public interface IGameModule
{
    /// <summary>Уникальный идентификатор (латиница, без пробелов).</summary>
    string Id { get; }

    /// <summary>Порядок карточки в каталоге (меньше — левее и выше).</summary>
    int CatalogOrder { get; }

    /// <summary>Название на главном экране.</summary>
    string Title { get; }

    /// <summary>Краткое описание для карточки каталога.</summary>
    string Description { get; }

    /// <summary>Иллюстрация (emoji или символ) для карточки.</summary>
    string IconEmoji { get; }

    /// <summary>Доступна ли игра для запуска.</summary>
    bool IsAvailable { get; }

    /// <summary>Показать метку «Новое» на карточке в каталоге.</summary>
    bool IsNew { get; }

    /// <summary>Фон игрового экрана из библиотеки Kenney.</summary>
    BackgroundTheme BackgroundTheme { get; }

    /// <summary>Создаёт экран игры.</summary>
    UserControl CreateView(INavigationContext navigation);
}
