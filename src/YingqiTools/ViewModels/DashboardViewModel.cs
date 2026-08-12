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
        KeyboardLockSession.SessionEnded += OnKeyboardSessionEnded;
        Refresh();
    }

    [RelayCommand]
    private void QuickLock()
    {
        bool started = KeyboardLockSession.TryStart(TimeSpan.FromMinutes(15));
        Notice = started
            ? "键盘已锁定 15 分钟，锁定窗口已显示，鼠标仍可用。"
            : KeyboardLockSession.LastError is { } error
                ? $"锁定失败：{error.Message}"
                : "键盘锁已经在运行，请使用现有锁定窗口。";
        Refresh();
    }

    [RelayCommand]
    private void ConfigureLid() => NavigationRequested?.Invoke(this, "lid");

    private void OnKeyboardSessionEnded(object? sender, EventArgs e)
    {
        Notice = "键盘已解锁，当前未锁定。";
        Refresh();
    }

    public void Refresh()
    {
        KeyboardStatus = KeyboardLockSession.IsRunning ? "运行中" : "未锁定";
        LidStatus = _lidControl.IsActive ? "已启用" : "未启用";
    }
}
