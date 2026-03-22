using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace switcher
{
    public partial class MainWindow : Window
    {
        private List<WindowItem> _items = new();
        private int _selectedIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
        }
        private ImageSource? GetIcon(IntPtr iconHandle)
        {
            if (iconHandle == IntPtr.Zero) return null;
            try
            {
                return Imaging.CreateBitmapSourceFromHIcon(
                    iconHandle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch { return null; }
        }

        public void ShowSwitcher()
        {
            var raw = WindowEnumerator.GetOpenWindows();
            _items = raw.Select(w => new WindowItem
            {
                Title = w.Title,
                ProcessName = w.ProcessName,
                Handle = w.Handle,
                Icon = GetIcon(w.IconHandle),
            }).ToList();

            _selectedIndex = 1 % _items.Count;
            WindowList.ItemsSource = _items;
            UpdateSelection();
            Visibility = Visibility.Visible;
        }

        public void HideSwitcher()
        {
            Visibility = Visibility.Hidden;
            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                FocusWindow(_items[_selectedIndex].Handle);
        }

        // switching functions

        public void CycleNext()
        {
            if (_items.Count == 0) return;
            _selectedIndex = (_selectedIndex + 1) % _items.Count;
            UpdateSelection();
        }

        public void CyclePrev()
        {
            if (_items.Count == 0) return;
            _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            WindowList.UpdateLayout();

            for (int i = 0; i < _items.Count; i++)
            {
                var container = (ContentPresenter?)WindowList.ItemContainerGenerator.ContainerFromIndex(i);
                if (container == null) continue;

                var border = FindChild<Border>(container);
                if (border == null) continue;

                border.BorderBrush = i == _selectedIndex ? Brushes.DodgerBlue : Brushes.Transparent;
            }
        }

        private void FocusWindow(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return;

            var foreThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            var appThread = GetCurrentThreadId();

            if (foreThread != appThread)
                AttachThreadInput(foreThread, appThread, true);

            ShowWindow(handle, Keyboard.SW_RESTORE);
            SetForegroundWindow(handle);
            BringWindowToTop(handle);

            if (foreThread != appThread)
                AttachThreadInput(foreThread, appThread, false);
        }

        // sorcery
        private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match) return match;
                var result = FindChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);
        [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")] private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}