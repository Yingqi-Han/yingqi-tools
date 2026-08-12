using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
        private KeyboardLockControl _control;
        public string Id { get { return "keyboard-lock"; } }
        public string Name { get { return "键盘锁"; } }
        public string Description { get { return "鼠标保持可用"; } }
        public string StatusSummary { get { return KeyboardLockSession.IsRunning ? "运行中" : "待命"; } }
        public Image Icon { get { return SystemIcons.Shield.ToBitmap(); } }
        public Control CreateControl()
        {
            if (_control == null) _control = new KeyboardLockControl();
            return _control;
        }
    }

    internal sealed class LidModule : IYingqiToolModule
    {
        private LidWorkModeControl _control;
        public string Id { get { return "lid-work-mode"; } }
        public string Name { get { return "合盖继续运行"; } }
        public string Description { get { return "仅本次会话生效"; } }
        public string StatusSummary { get { return _control != null && _control.IsActive ? "已启用" : "待命"; } }
        public Image Icon { get { return SystemIcons.Application.ToBitmap(); } }
        public Control CreateControl()
        {
            if (_control == null) _control = new LidWorkModeControl();
            return _control;
        }
        public bool RestoreAndWait()
        {
            if (_control == null && !File.Exists(GuardPaths.StateFile)) return true;
            return ((LidWorkModeControl)CreateControl()).RestoreAndWait(15000);
        }
    }

    internal static class Theme
    {
        public static readonly Color Sidebar = Color.FromArgb(14, 23, 38);
        public static readonly Color SidebarHover = Color.FromArgb(25, 38, 58);
        public static readonly Color SidebarActive = Color.FromArgb(31, 49, 74);
        public static readonly Color Accent = Color.FromArgb(61, 123, 253);
        public static readonly Color Canvas = Color.FromArgb(245, 247, 251);
        public static readonly Color Text = Color.FromArgb(20, 30, 47);
        public static readonly Color Muted = Color.FromArgb(127, 141, 161);

        public static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class BrandControl : Control
    {
        public BrandControl()
        {
            Dock = DockStyle.Top;
            Height = 118;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle mark = new Rectangle(24, 30, 48, 48);
            using (GraphicsPath path = Theme.RoundedRectangle(mark, 13))
            using (LinearGradientBrush brush = new LinearGradientBrush(mark, Color.FromArgb(82, 147, 255), Color.FromArgb(95, 88, 235), 45F))
                e.Graphics.FillPath(brush, path);
            using (Font logo = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Brush white = new SolidBrush(Color.White))
                e.Graphics.DrawString("Y", logo, white, new RectangleF(24, 31, 48, 47), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            using (Font title = new Font("Segoe UI", 13.5F, FontStyle.Bold))
            using (Font sub = new Font("Microsoft YaHei UI", 7.8F))
            using (Brush white = new SolidBrush(Color.White))
            using (Brush muted = new SolidBrush(Color.FromArgb(143, 158, 181)))
            {
                e.Graphics.DrawString("Yingqi Tools", title, white, 79, 31);
                e.Graphics.DrawString("个人 Windows 工具箱", sub, muted, 80, 60);
            }
        }
    }

    internal sealed class ModuleNavButton : Control
    {
        private bool _selected;
        private bool _hovered;
        private readonly IYingqiToolModule _module;
        public IYingqiToolModule Module { get { return _module; } }
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; Invalidate(); }
        }

        public ModuleNavButton(IYingqiToolModule module)
        {
            _module = module;
            Height = 72;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.PushButton;
            AccessibleName = module.Name;
            TabStop = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.Selectable, true);
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) { OnClick(EventArgs.Empty); e.Handled = true; }
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle body = new Rectangle(8, 3, Width - 16, Height - 6);
            Color background = _selected ? Theme.SidebarActive : (_hovered ? Theme.SidebarHover : Theme.Sidebar);
            using (GraphicsPath path = Theme.RoundedRectangle(body, 11))
            using (Brush brush = new SolidBrush(background)) e.Graphics.FillPath(brush, path);
            if (_selected)
            {
                using (GraphicsPath accent = Theme.RoundedRectangle(new Rectangle(8, 17, 4, 38), 2))
                using (Brush brush = new SolidBrush(Theme.Accent)) e.Graphics.FillPath(brush, accent);
            }

            Rectangle icon = new Rectangle(24, 18, 36, 36);
            using (GraphicsPath path = Theme.RoundedRectangle(icon, 10))
            using (Brush brush = new SolidBrush(_selected ? Color.FromArgb(55, 83, 126) : Color.FromArgb(27, 42, 63)))
                e.Graphics.FillPath(brush, path);
            using (Font iconFont = new Font("Segoe UI", 11F, FontStyle.Bold))
            using (Brush brush = new SolidBrush(_selected ? Color.FromArgb(128, 177, 255) : Color.FromArgb(127, 145, 171)))
            {
                string glyph = _module.Id == "keyboard-lock" ? "K" : "L";
                e.Graphics.DrawString(glyph, iconFont, brush, icon, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            using (Font title = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold))
            using (Font sub = new Font("Microsoft YaHei UI", 8F))
            using (Brush white = new SolidBrush(Color.FromArgb(239, 244, 252)))
            using (Brush muted = new SolidBrush(Color.FromArgb(139, 154, 177)))
            {
                e.Graphics.DrawString(_module.Name, title, white, 72, 14);
                e.Graphics.DrawString(_module.Description, sub, muted, 72, 39);
            }
            Color dotColor = _module.StatusSummary == "待命" ? Color.FromArgb(87, 104, 128) : Color.FromArgb(52, 211, 153);
            using (Brush dot = new SolidBrush(dotColor)) e.Graphics.FillEllipse(dot, Width - 29, 34, 7, 7);
        }
    }

    internal sealed class SidebarFooter : Control
    {
        public SidebarFooter()
        {
            Dock = DockStyle.Bottom;
            Height = 82;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen line = new Pen(Color.FromArgb(31, 44, 64))) e.Graphics.DrawLine(line, 24, 0, Width - 24, 0);
            using (Font title = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (Font sub = new Font("Microsoft YaHei UI", 8F))
            using (Brush titleBrush = new SolidBrush(Color.FromArgb(154, 172, 198)))
            using (Brush subBrush = new SolidBrush(Color.FromArgb(100, 117, 143)))
            {
                e.Graphics.DrawString("LOCAL  ·  PRIVATE", title, titleBrush, 24, 19);
                e.Graphics.DrawString("本地运行  ·  无遥测", sub, subBrush, 24, 43);
            }
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly Panel _content = new Panel();
        private readonly KeyboardModule _keyboard = new KeyboardModule();
        private readonly LidModule _lid = new LidModule();
        private readonly List<ModuleNavButton> _navigation = new List<ModuleNavButton>();
        private readonly System.Windows.Forms.Timer _statusTimer = new System.Windows.Forms.Timer();
        private readonly Icon _appIcon;
        private bool _allowClose;

        public MainForm()
        {
            Text = "Yingqi Tools";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1080, 700);
            MinimumSize = new Size(940, 640);
            BackColor = Theme.Canvas;
            Font = new Font("Microsoft YaHei UI", 10F);
            AutoScaleMode = AutoScaleMode.Dpi;
            _appIcon = AppIconFactory.Create();
            Icon = _appIcon;

            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 252, BackColor = Theme.Sidebar };
            sidebar.Controls.Add(new SidebarFooter());
            Panel navigationHost = new Panel { Dock = DockStyle.Top, Height = 164, Padding = new Padding(8, 0, 8, 0) };
            sidebar.Controls.Add(navigationHost);
            sidebar.Controls.Add(new BrandControl());

            AddModuleButton(navigationHost, _keyboard, 0);
            AddModuleButton(navigationHost, _lid, 78);

            _content.Dock = DockStyle.Fill;
            _content.Padding = new Padding(36, 28, 36, 28);
            _content.BackColor = Theme.Canvas;
            Controls.Add(_content);
            Controls.Add(sidebar);
            ShowModule(_keyboard);

            _statusTimer.Interval = 1000;
            _statusTimer.Tick += delegate { foreach (ModuleNavButton item in _navigation) item.Invalidate(); };
            _statusTimer.Start();
            FormClosing += OnFormClosing;
        }

        private void AddModuleButton(Panel host, IYingqiToolModule module, int top)
        {
            ModuleNavButton button = new ModuleNavButton(module);
            button.SetBounds(0, top, host.ClientSize.Width, 72);
            button.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            button.Click += delegate { ShowModule(module); };
            _navigation.Add(button);
            host.Controls.Add(button);
        }

        private void ShowModule(IYingqiToolModule module)
        {
            _content.SuspendLayout();
            _content.Controls.Clear();
            Control control = module.CreateControl();
            control.Dock = DockStyle.Fill;
            _content.Controls.Add(control);
            foreach (ModuleNavButton button in _navigation) button.Selected = button.Module.Id == module.Id;
            _content.ResumeLayout(true);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_allowClose) return;
            if (_lid.RestoreAndWait()) { _allowClose = true; return; }
            DialogResult result = MessageBox.Show("合盖设置尚未恢复。\n\n请保持窗口打开并重试，或选择“仍然退出”并在下次开机时由 PowerGuard 恢复。", "恢复尚未完成", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Retry) e.Cancel = true;
            else _allowClose = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _statusTimer.Dispose();
                if (_appIcon != null) _appIcon.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal static class AppIconFactory
    {
        [DllImport("user32.dll")] private static extern bool DestroyIcon(IntPtr handle);
        public static Icon Create()
        {
            using (Bitmap bitmap = new Bitmap(32, 32))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);
                using (GraphicsPath path = Theme.RoundedRectangle(new Rectangle(1, 1, 30, 30), 8))
                using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(1, 1, 30, 30), Color.FromArgb(69, 132, 255), Color.FromArgb(94, 86, 232), 45F))
                    graphics.FillPath(brush, path);
                using (Font font = new Font("Segoe UI", 17F, FontStyle.Bold, GraphicsUnit.Pixel))
                using (Brush brush = new SolidBrush(Color.White))
                    graphics.DrawString("Y", font, brush, new RectangleF(1, 1, 30, 29), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                IntPtr handle = bitmap.GetHicon();
                try { return (Icon)Icon.FromHandle(handle).Clone(); }
                finally { DestroyIcon(handle); }
            }
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
                if (!owns) { MessageBox.Show("Yingqi Tools 已在运行。", "Yingqi Tools", MessageBoxButtons.OK, MessageBoxIcon.Information); return 2; }
                try { SetProcessDPIAware(); } catch { }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
                return 0;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Yingqi Tools", MessageBoxButtons.OK, MessageBoxIcon.Error); return 1; }
            finally { if (_mutex != null) _mutex.Dispose(); }
        }
    }
}
