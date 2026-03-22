using System.Windows;

namespace switcher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void ShowSwitcher()
        {
            var windows = WindowEnumerator.GetOpenWindows();
            foreach (var w in windows)
                Console.WriteLine($"{w.ProcessName}: {w.Title}");

            Visibility = Visibility.Visible;
        }

        public void HideSwitcher()
        {
            Visibility = Visibility.Hidden;
        }

    }
}