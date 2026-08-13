using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LidWorkMode;
using YingqiTools.Pages;
using YingqiTools.ViewModels;
using Wpf.Ui.Controls;

namespace YingqiTools;

public partial class MainWindow : FluentWindow
{
    private readonly LidWorkModeControl _lidControl;
    private readonly Dictionary<string, Page> _pages;
    private bool _allowClose;
    private bool _restoring;

    public MainWindow(
        DashboardPage dashboard,
        KeyboardPage keyboard,
        LidPage lid,
        SettingsPage settings,
        LidWorkModeControl lidControl,
        DashboardViewModel dashboardViewModel)
    {
        InitializeComponent();
        _lidControl = lidControl;
        _pages = new Dictionary<string, Page>
        {
            ["overview"] = dashboard,
            ["keyboard"] = keyboard,
            ["lid"] = lid,
            ["settings"] = settings
        };
        dashboard.LidConfigurationRequested += (_, _) => Navigate("lid");
        Loaded += (_, _) =>
        {
            RootNavigation.ReplaceContent(dashboard);
            RootNavigation.ClearJournal();
        };
    }

    private void Overview_Click(object sender, RoutedEventArgs e) => Navigate("overview", false);
    private void Keyboard_Click(object sender, RoutedEventArgs e) => Navigate("keyboard", false);
    private void Lid_Click(object sender, RoutedEventArgs e) => Navigate("lid", false);
    private void Settings_Click(object sender, RoutedEventArgs e) => Navigate("settings", false);

    public void Navigate(string tag, bool selectItem = true)
    {
        if (!_pages.TryGetValue(tag, out Page? page)) return;
        if (selectItem)
        {
            NavigationViewItem? item = RootNavigation.MenuItems.OfType<NavigationViewItem>()
                .Concat(RootNavigation.FooterMenuItems.OfType<NavigationViewItem>())
                .FirstOrDefault(candidate => string.Equals(candidate.TargetPageTag, tag, StringComparison.Ordinal));
            item?.Activate(RootNavigation);
        }
        RootNavigation.ReplaceContent(page);
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
        if (_allowClose)
        {
            base.OnClosing(e);
            return;
        }

        if (_restoring)
        {
            e.Cancel = true;
            return;
        }
        if (!_lidControl.RequiresRecovery)
        {
            _allowClose = true;
            base.OnClosing(e);
            return;
        }

        e.Cancel = true;
        _restoring = true;
        IsEnabled = false;
        bool restored = await _lidControl.RestoreAsync(TimeSpan.FromSeconds(15));
        IsEnabled = true;
        _restoring = false;
        if (restored)
        {
            _allowClose = true;
            _ = Dispatcher.BeginInvoke(Close);
            return;
        }

        System.Windows.MessageBox.Show(
            "合盖设置尚未恢复，请保持工具箱打开并点击“立即恢复原设置”后再退出。下次开机的 PowerGuard 任务也会恢复残留状态。",
            "恢复尚未完成",
            System.Windows.MessageBoxButton.OK,
            MessageBoxImage.Warning);
        Navigate("lid");
    }
}
