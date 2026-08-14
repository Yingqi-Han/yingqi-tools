using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using KeyboardCoolDownLock;
using LidWorkMode;
using YingqiClipboard;
using Microsoft.Extensions.DependencyInjection;
using YingqiTools.Pages;
using YingqiTools.Services;
using YingqiTools.ViewModels;

namespace YingqiTools;

public partial class App : Application
{
    private Mutex? _singleInstance;
    private ServiceProvider? _services;
    private ClipboardHistorySession? _clipboardSession;

    protected override void OnStartup(StartupEventArgs e)
    {
        string? diagnosticPath = ReadArgument(e.Args, "--diagnostic-file");
        if (e.Args.Contains("--self-test", StringComparer.OrdinalIgnoreCase))
        {
            int exitCode = RunSelfTest();
            Shutdown(exitCode);
            return;
        }

        if (e.Args.Contains("--clipboard-smoke-test", StringComparer.OrdinalIgnoreCase))
        {
            string? smokeDataDirectory = ReadArgument(e.Args, "--clipboard-smoke-data");
            _ = CompleteClipboardSmokeTestAsync(diagnosticPath, smokeDataDirectory);
            return;
        }

        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            WriteDiagnostic(diagnosticPath, args.Exception);
            args.Handled = false;
        };

        if (!e.Args.Contains("--allow-multiple", StringComparer.OrdinalIgnoreCase))
        {
            _singleInstance = new Mutex(true, "Local\\YingqiTools.SingleInstance", out bool owns);
            if (!owns)
            {
                System.Windows.MessageBox.Show("Yingqi Tools 已经在运行。", "Yingqi Tools", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown(2);
                return;
            }
        }

        try
        {
            ServiceCollection services = new();
            services.AddSingleton<SettingsService>();
            services.AddSingleton<KeyboardLockControl>();
            services.AddSingleton<LidWorkModeControl>();
            string clipboardData = Path.Combine(AppContext.BaseDirectory, "Data", "ClipboardHistory");
            services.AddSingleton(new ClipboardHistoryOptions { DataDirectory = clipboardData });
            services.AddSingleton<ClipboardHistorySession>();
            services.AddSingleton(provider => new ClipboardHistoryControl(provider.GetRequiredService<ClipboardHistorySession>()));
            services.AddSingleton<ClipboardWindowService>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<DashboardPage>();
            services.AddSingleton<KeyboardPage>();
            services.AddSingleton<LidPage>();
            services.AddSingleton<ClipboardPage>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<MainWindow>();
            _services = services.BuildServiceProvider();

            MainWindow window = _services.GetRequiredService<MainWindow>();
            _clipboardSession = _services.GetRequiredService<ClipboardHistorySession>();
            MainWindow = window;
            _services.GetRequiredService<SettingsService>().ApplyTheme(window);
            window.Show();
            _ = StartClipboardAsync(_clipboardSession, diagnosticPath);
        }
        catch (Exception ex)
        {
            WriteDiagnostic(diagnosticPath, ex);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }

    private static async Task StartClipboardAsync(ClipboardHistorySession session, string? diagnosticPath)
    {
        try { await session.StartAsync(); }
        catch (Exception ex) { WriteDiagnostic(diagnosticPath, ex); }
    }

    private static int RunSelfTest()
    {
        try
        {
            PowerPlanService.ReadCurrent();
            return KeyboardLockSession.SelfTest() ? 0 : 1;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return 1;
        }
    }

    private static async Task<int> RunClipboardSmokeTestAsync(string dataDirectory)
    {
        try
        {
            ClipboardHistoryOptions options = new() { DataDirectory = dataDirectory };
            await using ClipboardHistorySession session = new(options);
            await session.StartAsync();
            return session.SyncState is ClipboardSyncState.Ready or ClipboardSyncState.HistoryDisabled or ClipboardSyncState.AccessDenied ? 0 : 1;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return 1;
        }
    }

    private async Task CompleteClipboardSmokeTestAsync(string? diagnosticPath, string? dataDirectory)
    {
        int exitCode;
        try
        {
            if (string.IsNullOrWhiteSpace(dataDirectory)) throw new ArgumentException("--clipboard-smoke-data is required.");
            exitCode = await RunClipboardSmokeTestAsync(Path.GetFullPath(dataDirectory));
        }
        catch (Exception ex)
        {
            WriteDiagnostic(diagnosticPath, ex);
            exitCode = 1;
        }
        Shutdown(exitCode);
    }

    private static string? ReadArgument(string[] args, string key)
    {
        for (int index = 0; index < args.Length - 1; index++)
            if (string.Equals(args[index], key, StringComparison.OrdinalIgnoreCase)) return args[index + 1];
        return null;
    }

    private static void WriteDiagnostic(string? path, Exception exception)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try { File.WriteAllText(path, exception.ToString()); }
        catch { }
    }
}
