using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HoroshieIgry.Core.UI;

/// <summary>Преобразует цвет в формате #RRGGBB в кисть WPF.</summary>
public class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex)!;
                return new SolidColorBrush(color);
            }
            catch
            {
                // ignored
            }
        }

        return new SolidColorBrush(Color.FromRgb(0xFF, 0xF8, 0xE7));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
