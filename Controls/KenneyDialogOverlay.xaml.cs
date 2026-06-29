using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace HoroshieIgry.Controls;

/// <summary>Модальный диалог в стиле Kenney поверх экрана игры.</summary>
public partial class KenneyDialogOverlay : UserControl, INotifyPropertyChanged
{
    private TaskCompletionSource<bool>? _tcs;
    private string _title = string.Empty;
    private string _message = string.Empty;
    private string _yesLabel = "Да";
    private string _noLabel = "Нет";

    public string Title
    {
        get => _title;
        private set { _title = value; OnPropertyChanged(); }
    }

    public string Message
    {
        get => _message;
        private set { _message = value; OnPropertyChanged(); }
    }

    public string YesLabel
    {
        get => _yesLabel;
        private set { _yesLabel = value; OnPropertyChanged(); }
    }

    public string NoLabel
    {
        get => _noLabel;
        private set { _noLabel = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public KenneyDialogOverlay()
    {
        InitializeComponent();
        DataContext = this;
    }

    public Task<bool> ShowAsync(string title, string message, string yesLabel, string noLabel)
    {
        Title = title;
        Message = message;
        YesLabel = yesLabel;
        NoLabel = noLabel;

        _tcs = new TaskCompletionSource<bool>();
        Visibility = Visibility.Visible;
        return _tcs.Task;
    }

    private void YesButton_Click(object sender, RoutedEventArgs e) => Complete(true);

    private void NoButton_Click(object sender, RoutedEventArgs e) => Complete(false);

    private void Complete(bool result)
    {
        Visibility = Visibility.Collapsed;
        _tcs?.TrySetResult(result);
        _tcs = null;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
