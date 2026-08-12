using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyboardCoolDownLock;
using LidWorkMode;

namespace YingqiTools.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly LidWorkModeControl _lidControl;

    [ObservableProperty]
    private string _keyboardStatus = "未锁定";

    [ObservableProperty]
    private string _lidStatus = "未启用";

    [ObservableProperty]
    private string _notice = "所有临时功能均未启用。";

    public event EventHandler<string>? NavigationRequested;

    public DashboardViewModel(LidWorkModeControl lidControl)
    {
        _lidControl = lidControl;
        KeyboardLockSession.SessionEnded += (_, _) => Refresh();
        Refresh();
    }

    [RelayCommand]
    private void QuickLock()
    {
        bool started = KeyboardLockSession.TryStart(TimeSpan.FromMinutes(15));
        Notice = started ? "键盘已锁定 15 分钟，鼠标仍可用。" : "键盘锁已经在运行。";
        Refresh();
    }

    [RelayCommand]
    private void ConfigureLid() => NavigationRequested?.Invoke(this, "lid");

    public void Refresh()
    {
        KeyboardStatus = KeyboardLockSession.IsRunning ? "运行中" : "未锁定";
        LidStatus = _lidControl.IsActive ? "已启用" : "未启用";
    }
}
