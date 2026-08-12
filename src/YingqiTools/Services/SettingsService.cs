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
    }

    private readonly string _settingsPath;
    public ThemePreference Theme { get; private set; }

    public SettingsService()
    {
        _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YingqiTools", "settings.json");
        Theme = LoadTheme(_settingsPath);
    }

    public SettingsService(string settingsPath)
    {
        _settingsPath = settingsPath;
        Theme = LoadTheme(_settingsPath);
    }

    public void SetTheme(ThemePreference preference, Window? window = null)
    {
        Theme = preference;
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        JsonSerializerOptions options = new() { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        string json = JsonSerializer.Serialize(new SettingsDocument { Theme = preference }, options);
        File.WriteAllText(_settingsPath, json);
        if (window is not null) ApplyTheme(window);
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

    private static ThemePreference LoadTheme(string path)
    {
        try
        {
            if (!File.Exists(path)) return ThemePreference.System;
            JsonSerializerOptions options = new();
            options.Converters.Add(new JsonStringEnumConverter());
            SettingsDocument? document = JsonSerializer.Deserialize<SettingsDocument>(File.ReadAllText(path), options);
            return document?.Theme ?? ThemePreference.System;
        }
        catch
        {
            return ThemePreference.System;
        }
    }
}
