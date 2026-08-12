using CommunityToolkit.Mvvm.ComponentModel;
using YingqiTools.Models;
using YingqiTools.Services;

namespace YingqiTools.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;

    [ObservableProperty]
    private ThemePreference _selectedTheme;

    public IReadOnlyList<ThemePreference> Themes { get; } = Enum.GetValues<ThemePreference>();

    public SettingsViewModel(SettingsService settings)
    {
        _settings = settings;
        _selectedTheme = settings.Theme;
    }

    partial void OnSelectedThemeChanged(ThemePreference value)
    {
        if (System.Windows.Application.Current?.MainWindow is { } window)
            _settings.SetTheme(value, window);
    }
}
