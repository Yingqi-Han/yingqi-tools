using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyboardCoolDownLock;
using LidWorkMode;
using YingqiClipboard;

namespace YingqiTools.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly LidWorkModeControl _lidControl;
    private readonly ClipboardHistorySession _clipboardSession;

    [ObservableProperty]
    private string _keyboardStatus = "未锁定";

    [ObservableProperty]
    private string _lidStatus = "未启用";

    [ObservableProperty]
    private string _clipboardStatus = "正在准备";

    [ObservableProperty]
    private string _notice = "所有临时功能均未启用。";

    public event EventHandler<string>? NavigationRequested;
    public event EventHandler? ClipboardWindowRequested;

    public DashboardViewModel(LidWorkModeControl lidControl, ClipboardHistorySession clipboardSession)
    {
        _lidControl = lidControl;
        _clipboardSession = clipboardSession;
        KeyboardLockSession.SessionEnded += OnKeyboardSessionEnded;
        _clipboardSession.EntriesChanged += (_, _) => RefreshOnUiThread();
        _clipboardSession.StateChanged += (_, _) => RefreshOnUiThread();
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

    [RelayCommand]
    private void OpenClipboard() => NavigationRequested?.Invoke(this, "clipboard");

    [RelayCommand]
    private void OpenClipboardWindow() => ClipboardWindowRequested?.Invoke(this, EventArgs.Empty);

    private void OnKeyboardSessionEnded(object? sender, EventArgs e)
    {
        Notice = "键盘已解锁，当前未锁定。";
        Refresh();
    }

    public void Refresh()
    {
        KeyboardStatus = KeyboardLockSession.IsRunning ? "运行中" : "未锁定";
        LidStatus = _lidControl.IsActive ? "已启用" : "未启用";
        ClipboardStatus = _clipboardSession.SyncState switch
        {
            ClipboardSyncState.Ready => $"{_clipboardSession.Count} 条",
            ClipboardSyncState.HistoryDisabled => "Win+V 未开启",
            ClipboardSyncState.AccessDenied => "访问受限",
            _ => _clipboardSession.Count > 0 ? $"{_clipboardSession.Count} 条" : "正在准备"
        };
    }

    private void RefreshOnUiThread()
    {
        if (System.Windows.Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
        {
            _ = dispatcher.BeginInvoke(Refresh);
            return;
        }
        Refresh();
    }
}
