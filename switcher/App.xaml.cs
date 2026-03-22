using System.Windows;

namespace switcher
{
    public partial class App : Application
    {
        private Keyboard? _hook;
        private MainWindow? _window;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _window = new MainWindow();

            _hook = new Keyboard();
            _hook.OnAltTabPressed += () => _window.ShowSwitcher();
            _hook.OnAltReleased += () => _window.HideSwitcher();
            _hook.Install();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            _hook?.Dispose();
            base.OnExit(e);
        }
    }
}