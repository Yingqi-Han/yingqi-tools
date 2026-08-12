using System.Text.Json;
using YingqiTools.Models;
using YingqiTools.Services;
using Xunit;

namespace YingqiTools.Tests;

public sealed class SettingsServiceTests
{
    [Fact]
    public void MissingSettings_DefaultsToSystem()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "settings.json");
        SettingsService service = new(path);
        Assert.Equal(ThemePreference.System, service.Theme);
    }

    [Fact]
    public void InvalidSettings_DefaultsToSystem()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "settings.json");
        File.WriteAllText(path, "not-json");
        SettingsService service = new(path);
        Assert.Equal(ThemePreference.System, service.Theme);
    }

    [Fact]
    public void SaveContainsThemeOnly()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "settings.json");
        SettingsService service = new(path);
        service.SetTheme(ThemePreference.Dark);
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Single(document.RootElement.EnumerateObject());
        Assert.Equal("Dark", document.RootElement.GetProperty("Theme").GetString());
    }
}
