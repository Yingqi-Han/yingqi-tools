using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using KeyboardCoolDownLock;
using LidWorkMode;

namespace YingqiTools
{
    public interface IYingqiToolModule
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        string StatusSummary { get; }
        Image Icon { get; }
        Control CreateControl();
    }

    internal sealed class KeyboardModule : IYingqiToolModule
    {
        public string Id { get { return "keyboard-lock"; } }
        public string Name { get { return "\u952e\u76d8\u9501"; } }
        public string Description { get { return "\u9501\u952e\u76d8，\u4fdd\u7559\u9f20\u6807"; } }
        public string StatusSummary { get { return KeyboardLockSession.IsRunning ? "\u8fd0\u884c\u4e2d" : "\u672a启\u7528"; } }
        public Image Icon { get { return SystemIcons.Shield.ToBitmap(); } }
        public Control CreateControl() { return new KeyboardLockControl(); }
    }

    internal sealed class LidModule : IYingqiToolModule
    {
        private LidWorkModeControl _control;
        public string Id { get { return "lid-work-mode"; } }
        public string Name { get { return "\u5408\u76d6\u7ee7\u7eed\u8fd0\u884c"; } }
        public string Description { get { return "\u672c\u6b21\u4f1a\u8bdd临\u65f6\u751f\u6548"; } }
        public string StatusSummary { get { return _control != null && _control.IsActive ? "\u5df2启\u7528" : "\u672a启\u7528"; } }
        public Image Icon { get { return SystemIcons.Application.ToBitmap(); } }
        public Control CreateControl() { if (_control == null) _control = new LidWorkModeControl(); return _control; }
        public bool RestoreAndWait()
        {
            if (_control == null && !System.IO.File.Exists(GuardPaths.StateFile)) return true;
            return ((LidWorkModeControl)CreateControl()).RestoreAndWait(15000);
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly Panel _content = new Panel();
        private readonly LidModule _lid = new LidModule();
        private bool _allowClose;

        public MainForm()
        {
            Text = "Yingqi Tools";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(860, 540);
            MinimumSize = new Size(780, 500);
            BackColor = Color.FromArgb(243, 244, 246);
            Font = new Font("Microsoft YaHei UI", 10F);
            Icon = SystemIcons.Application;

            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 225, BackColor = Color.FromArgb(17, 24, 39) };
            Label brand = new Label { Text = "Yingqi Tools", ForeColor = Color.White, Font = new Font("Microsoft YaHei UI", 20F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 82 };
            sidebar.Controls.Add(brand);
            AddModuleButton(sidebar, new KeyboardModule(), 95);
            AddModuleButton(sidebar, _lid, 165);
            Label footer = new Label { Text = "Local only · No telemetry", ForeColor = Color.FromArgb(156, 163, 175), TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Bottom, Height = 42 };
            sidebar.Controls.Add(footer);

            _content.Dock = DockStyle.Fill;
            _content.Padding = new Padding(18);
            Controls.Add(_content);
            Controls.Add(sidebar);
            ShowModule(new KeyboardModule());
            FormClosing += OnFormClosing;
        }

        private void AddModuleButton(Panel sidebar, IYingqiToolModule module, int top)
        {
            Button button = new Button { Text = module.Name + "\n" + module.Description, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.White, BackColor = Color.FromArgb(31, 41, 55), FlatStyle = FlatStyle.Flat };
            button.FlatAppearance.BorderSize = 0; button.SetBounds(12, top, 201, 58); button.Click += delegate { ShowModule(module); };
            sidebar.Controls.Add(button);
        }

        private void ShowModule(IYingqiToolModule module)
        {
            _content.Controls.Clear();
            Control control = module.CreateControl();
            control.Dock = DockStyle.Fill;
            _content.Controls.Add(control);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_allowClose) return;
            if (_lid.RestoreAndWait()) { _allowClose = true; return; }
            DialogResult result = MessageBox.Show("\u5408\u76d6\u8bbe\u7f6e\u5c1a\u672a\u6062\u590d\u3002\n\n\u8bf7保\u6301窗\u53e3打\u5f00并重\u8bd5\uff0c或选\u62e9“仍\u7136\u9000\u51fa”并在下次开\u673a时由 PowerGuard 恢\u590d\u3002", "\u6062\u590d尚\u672a\u5b8c\u6210", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Retry) { e.Cancel = true; } else { _allowClose = true; }
        }
    }

    internal static class Program
    {
        private static Mutex _mutex;
        [DllImport("user32.dll")] private static extern bool SetProcessDPIAware();
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0] == "--self-test") { PowerPlanService.ReadCurrent(); KeyboardLockSession.SelfTest(); return 0; }
                bool owns;
                _mutex = new Mutex(true, "Local\\YingqiTools.SingleInstance", out owns);
                if (!owns) { MessageBox.Show("Yingqi Tools \u5df2\u5728\u8fd0\u884c\u3002", "Yingqi Tools", MessageBoxButtons.OK, MessageBoxIcon.Information); return 2; }
                try { SetProcessDPIAware(); } catch { }
                Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm()); return 0;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Yingqi Tools", MessageBoxButtons.OK, MessageBoxIcon.Error); return 1; }
            finally { if (_mutex != null) _mutex.Dispose(); }
        }
    }
}
