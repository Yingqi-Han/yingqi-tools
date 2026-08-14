using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using YingqiTools.Models;

namespace YingqiTools.Services;

public sealed class SettingsService
{
    private sealed class SettingsDocument
    {
        public ThemePreference Theme { get; set; } = ThemePreference.System;
        public bool ClipboardWindowTopmost { get; set; } = true;
    }

    private readonly string _settingsPath;
    public ThemePreference Theme { get; private set; }
    public bool ClipboardWindowTopmost { get; private set; }

    public SettingsService()
    {
        _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YingqiTools", "settings.json");
        (Theme, ClipboardWindowTopmost) = Load(_settingsPath);
    }

    public SettingsService(string settingsPath)
    {
        _settingsPath = settingsPath;
        (Theme, ClipboardWindowTopmost) = Load(_settingsPath);
    }

    public void SetTheme(ThemePreference preference, Window? window = null)
    {
        Theme = preference;
        Save();
        if (window is not null) ApplyTheme(window);
    }

    public void SetClipboardWindowTopmost(bool value)
    {
        ClipboardWindowTopmost = value;
        Save();
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        JsonSerializerOptions options = new() { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        string json = JsonSerializer.Serialize(new SettingsDocument
        {
            Theme = Theme,
            ClipboardWindowTopmost = ClipboardWindowTopmost
        }, options);
        string temporaryPath = _settingsPath + ".tmp";
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, _settingsPath, true);
    }

    public void ApplyTheme(Window window)
    {
        if (!window.IsLoaded)
        {
            window.Loaded += (_, _) => ApplyTheme(window);
            return;
        }
        try { SystemThemeWatcher.UnWatch(window); }
        catch (InvalidOperationException) { }
        if (Theme == ThemePreference.System)
        {
            SystemThemeWatcher.Watch(window, WindowBackdropType.Mica, true);
            return;
        }

        ApplicationThemeManager.Apply(
            Theme == ThemePreference.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light,
            WindowBackdropType.Mica,
            true);
    }

    private static (ThemePreference Theme, bool ClipboardWindowTopmost) Load(string path)
    {
        try
        {
            if (!File.Exists(path)) return (ThemePreference.System, true);
            JsonSerializerOptions options = new();
            options.Converters.Add(new JsonStringEnumConverter());
            SettingsDocument? document = JsonSerializer.Deserialize<SettingsDocument>(File.ReadAllText(path), options);
            return (document?.Theme ?? ThemePreference.System, document?.ClipboardWindowTopmost ?? true);
        }
        catch
        {
            return (ThemePreference.System, true);
        }
    }
}
