namespace HoroshieIgry.Core.UI;

/// <summary>
/// Пути к ассетам Kenney UI Pack.
/// Перед добавлением нового элемента интерфейса — ищите подходящий файл здесь.
/// </summary>
public static class KenneyPaths
{
    private const string Root = "Assets/UI/Kenney/Vector";

    public static string InPalette(KenneyPalette palette, string fileName)
        => $"{Root}/{palette}/{fileName}";

    public static string InExtra(string fileName)
        => $"{Root}/Extra/{fileName}";

    // —— Кнопки ——
    public static string ButtonRectangleGreen => InPalette(KenneyPalette.Green, "button_rectangle_depth_flat.svg");
    public static string ButtonRectangleBlue => InPalette(KenneyPalette.Blue, "button_rectangle_depth_flat.svg");
    public static string ButtonRectangleGrey => InPalette(KenneyPalette.Grey, "button_rectangle_depth_flat.svg");
    public static string ButtonRoundBlue => InPalette(KenneyPalette.Blue, "button_round_depth_flat.svg");

    // —— Панели / поля ввода ——
    public static string PanelRectangle => InExtra("input_rectangle.svg");
    public static string PanelSquare => InExtra("input_square.svg");
    public static string PanelOutlineRectangle => InExtra("input_outline_rectangle.svg");

    // —— Иконки и декор ——
    public static string IconRepeat => InExtra("icon_repeat_dark.svg");
    public static string IconPlay => InExtra("icon_play_dark.svg");
    public static string ArrowBack => InPalette(KenneyPalette.Blue, "arrow_basic_w_small.svg");
    public static string Star => InPalette(KenneyPalette.Yellow, "star.svg");
    public static string IconCircle => InPalette(KenneyPalette.Blue, "icon_circle.svg");
    public static string IconSettings => InPalette(KenneyPalette.Blue, "slide_horizontal_color_section.svg");
    public static string Divider => InExtra("divider.svg");

    // —— Карточки игры «Память» ——
    public static string MemoryCardBack => InPalette(KenneyPalette.Blue, "button_square_depth_flat.svg");
    public static string MemoryCardFront => InExtra("input_square.svg");
}
