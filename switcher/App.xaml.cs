using System.Windows;

namespace switcher
{
    public partial class App : System.Windows.Application
    {
        private Keyboard? _hook;
        private MainWindow? _window;
        private NotifyIcon? _trayIcon;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _window = new MainWindow();

            _hook = new Keyboard();
            _hook.OnAltTabPressed += () => {
                if (_window.Visibility == System.Windows.Visibility.Visible)
                    _window.CycleNext();
                else
                    _window.ShowSwitcher();
            };

            _hook.OnShiftTabPressed += () => _window.CyclePrev();
            _hook.OnAltReleased += () => _window.HideSwitcher();
            _hook.Install();

            _trayIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "Switcher"
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Exit", null, (_, _) =>
            {
                _trayIcon.Visible = false;
                Shutdown();
            });

            _trayIcon.ContextMenuStrip = menu;
        }
        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            _hook?.Dispose();
            base.OnExit(e);
        }
    }
}