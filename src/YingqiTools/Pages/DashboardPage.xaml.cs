using System.Windows.Controls;
using YingqiTools.ViewModels;

namespace YingqiTools.Pages;

public partial class DashboardPage : Page
{
    public event EventHandler? LidConfigurationRequested;
    public event EventHandler? ClipboardRequested;
    public event EventHandler? ClipboardWindowRequested;
    public event EventHandler? SettingsRequested;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        IsVisibleChanged += (_, _) => { if (IsVisible) viewModel.Refresh(); };
    }

    private void ConfigureLid_Click(object sender, System.Windows.RoutedEventArgs e) => LidConfigurationRequested?.Invoke(this, EventArgs.Empty);
    private void OpenClipboard_Click(object sender, System.Windows.RoutedEventArgs e) => ClipboardRequested?.Invoke(this, EventArgs.Empty);
    private void OpenClipboardWindow_Click(object sender, System.Windows.RoutedEventArgs e) => ClipboardWindowRequested?.Invoke(this, EventArgs.Empty);
    private void OpenSettings_Click(object sender, System.Windows.RoutedEventArgs e) => SettingsRequested?.Invoke(this, EventArgs.Empty);
}
