using System.Windows;
using YingqiClipboard;

namespace YingqiTools.Services;

public sealed class ClipboardWindowService
{
    private readonly ClipboardHistorySession _session;
    private readonly SettingsService _settings;
    private ClipboardCompactWindow? _window;

    public ClipboardWindowService(ClipboardHistorySession session, SettingsService settings)
    {
        _session = session;
        _settings = settings;
    }

    public void Show()
    {
        if (_window is null)
        {
            _window = new ClipboardCompactWindow(_session, _settings.ClipboardWindowTopmost);
            _window.TopmostPreferenceChanged += (_, value) => _settings.SetClipboardWindowTopmost(value);
            _window.Closed += (_, _) => _window = null;
        }
        if (_window.WindowState == WindowState.Minimized) _window.WindowState = WindowState.Normal;
        _window.Show();
        _window.Activate();
    }

    public void Close() => _window?.Close();
}
