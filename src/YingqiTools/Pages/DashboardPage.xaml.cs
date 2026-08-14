using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YingqiTools.Services;
using YingqiTools.ViewModels;

namespace YingqiTools.Pages;

public partial class DashboardPage : Page
{
    private Window? _hostWindow;

    public event EventHandler? LidConfigurationRequested;
    public event EventHandler? ClipboardRequested;
    public event EventHandler? ClipboardWindowRequested;
    public event EventHandler? SettingsRequested;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        DashboardScroller.AddHandler(
            Mouse.PreviewMouseWheelEvent,
            new MouseWheelEventHandler(DashboardScroller_PreviewMouseWheel),
            handledEventsToo: true);
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        IsVisibleChanged += (_, _) => { if (IsVisible) viewModel.Refresh(); };
    }

    private void DashboardScroller_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta == 0 || DashboardScroller.ScrollableHeight <= 0) return;
        double previousOffset = DashboardScroller.VerticalOffset;
        double targetOffset = WheelScrollHelper.GetTargetOffset(previousOffset, DashboardScroller.ScrollableHeight, e.Delta);
        if (targetOffset.Equals(previousOffset)) return;
        DashboardScroller.ScrollToVerticalOffset(targetOffset);
        e.Handled = true;
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
        DashboardScroller.Height = Math.Max(460, _hostWindow.ActualHeight - 84);
    }

    private void ConfigureLid_Click(object sender, System.Windows.RoutedEventArgs e) => LidConfigurationRequested?.Invoke(this, EventArgs.Empty);
    private void OpenClipboard_Click(object sender, System.Windows.RoutedEventArgs e) => ClipboardRequested?.Invoke(this, EventArgs.Empty);
    private void OpenClipboardWindow_Click(object sender, System.Windows.RoutedEventArgs e) => ClipboardWindowRequested?.Invoke(this, EventArgs.Empty);
    private void OpenSettings_Click(object sender, System.Windows.RoutedEventArgs e) => SettingsRequested?.Invoke(this, EventArgs.Empty);
}
