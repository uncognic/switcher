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
        private const int SW_RESTORE = 9;
        private const int SW_SHOWMINIMIZED = 2;

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

        public async void HideSwitcher()
        {
            Visibility = Visibility.Hidden;

            if (_selectedIndex < 0 || _selectedIndex >= _items.Count)
                return;

            var handle = _items[_selectedIndex].Handle;
            await Task.Delay(50);
            FocusWindow(handle);
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
            for (int i = 0; i < _items.Count; i++)
                _items[i].IsSelected = i == _selectedIndex;
        }

        private void FocusWindow(IntPtr handle)
        {
            if (handle == IntPtr.Zero || !IsWindow(handle))
                return;

            var foreground = GetForegroundWindow();
            var foreThread = foreground == IntPtr.Zero ? 0 : GetWindowThreadProcessId(foreground, IntPtr.Zero);
            var targetThread = GetWindowThreadProcessId(handle, IntPtr.Zero);
            var appThread = GetCurrentThreadId();

            var attachedFore = false;
            var attachedTarget = false;

            try
            {
                if (foreThread != 0 && foreThread != appThread)
                    attachedFore = AttachThreadInput(foreThread, appThread, true);

                if (targetThread != 0 && targetThread != appThread && targetThread != foreThread)
                    attachedTarget = AttachThreadInput(targetThread, appThread, true);

                WINDOWPLACEMENT placement = new();
                placement.length = Marshal.SizeOf(placement);
                if (GetWindowPlacement(handle, ref placement) && placement.showCmd == SW_SHOWMINIMIZED)
                    ShowWindow(handle, SW_RESTORE);

                BringWindowToTop(handle);
                SetForegroundWindow(handle);
            }
            finally
            {
                if (attachedTarget)
                    AttachThreadInput(targetThread, appThread, false);

                if (attachedFore)
                    AttachThreadInput(foreThread, appThread, false);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);
        [DllImport("user32.dll")] private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")] private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr hWnd);
    }
}