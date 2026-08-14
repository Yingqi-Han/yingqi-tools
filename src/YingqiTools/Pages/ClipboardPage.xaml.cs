using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YingqiClipboard;
using YingqiTools.Services;

namespace YingqiTools.Pages;

public partial class ClipboardPage : Page
{
    private readonly ClipboardHistoryControl _control;
    private Window? _hostWindow;

    public ClipboardPage(ClipboardHistoryControl control, ClipboardWindowService windowService)
    {
        InitializeComponent();
        _control = control;
        ComponentHost.Content = control;
        control.OpenCompactWindowRequested += (_, _) => windowService.Show();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void Page_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_control.ScrollByWheelDelta(e.Delta)) e.Handled = true;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _hostWindow = Window.GetWindow(this);
        if (_hostWindow is null) return;
        _hostWindow.SizeChanged += HostWindow_SizeChanged;
        UpdateAvailableHeight();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_hostWindow is not null) _hostWindow.SizeChanged -= HostWindow_SizeChanged;
        _hostWindow = null;
    }

    private void HostWindow_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateAvailableHeight();

    private void UpdateAvailableHeight()
    {
        if (_hostWindow is null) return;
        ComponentHost.Height = Math.Max(420, _hostWindow.ActualHeight - 84);
    }
}
