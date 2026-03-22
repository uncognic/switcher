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
            Visibility = Visibility.Visible;
        }

        public void HideSwitcher()
        {
            Visibility = Visibility.Hidden;
        }

    }
}