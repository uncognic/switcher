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
            _hook.OnAltTabPressed += () => {
                if (_window.Visibility == System.Windows.Visibility.Visible)
                    _window.CycleNext();
                else
                    _window.ShowSwitcher();
            };
            _hook.OnAltReleased += () => _window.HideSwitcher();
            _hook.OnShiftTabPressed += () => _window.CyclePrev();
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