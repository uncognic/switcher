using System.Runtime.InteropServices;
using System.Text;

namespace switcher
{
    public class AppWindow
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = "";
        public string ProcessName { get; set; } = "";
        public IntPtr IconHandle { get; set; }
    }

    public static class WindowEnumerator
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const uint GW_OWNER = 4;
        private const int WM_GETICON = 0x007F;
        private const int ICON_BIG = 1;
        private const int ICON_SMALL2 = 2;
        private const int GCLP_HICON = -14;
        public static List<AppWindow> GetOpenWindows()
        {
            
            var windows = new List<AppWindow>();

            EnumWindows((hWnd, _) =>
            {
                if (!IsWindowVisible(hWnd))
                {
                    return true;
                }

                if (!isAltTabWindow(hWnd))
                {
                    return true;
                }

                var title = GetWindowTitle(hWnd);
                if (string.IsNullOrWhiteSpace(title)) return true;

                var processName = GetProcessName(hWnd);
                var icon = GetWindowIcon(hWnd);

                windows.Add(new AppWindow
                {
                    Handle = hWnd,
                    Title = title,
                    ProcessName = processName,
                    IconHandle = icon
                });

                return true;
            }, IntPtr.Zero);
            return windows;
        }

        private static readonly HashSet<string> _blacklist = new(StringComparer.OrdinalIgnoreCase)
        {
            "TextInputHost",
            "ApplicationFrameHost",
            "SearchHost",
            "ShellExperienceHost",
            "StartMenuExperienceHost",
            "LockApp",
            "RtkUwp"
        };

        private static bool isAltTabWindow(IntPtr hWnd)
        {
            var exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);

            if ((exStyle & WS_EX_TOOLWINDOW) != 0) return false;
            if ((exStyle & WS_EX_NOACTIVATE) != 0) return false;

            var owner = GetWindow(hWnd, GW_OWNER);
            if (owner != IntPtr.Zero) return false;

            GetWindowThreadProcessId(hWnd, out uint pid);
            try
            {
                var proc = System.Diagnostics.Process.GetProcessById((int)pid);
                if (_blacklist.Contains(proc.ProcessName)) return false;
            }
            catch { return false; }

            return true;
        }

        private static string GetWindowTitle(IntPtr hWnd)
        {
            var sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
        private static string GetProcessName(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint pid);
            try
            {
                var proc = System.Diagnostics.Process.GetProcessById((int)pid);
                return proc.ProcessName;
            }
            catch { return ""; }
        }
        private static IntPtr GetWindowIcon(IntPtr hWnd)
        {
            var icon = SendMessage(hWnd, WM_GETICON, ICON_BIG, 0);
            if (icon == IntPtr.Zero)
                icon = SendMessage(hWnd, WM_GETICON, ICON_SMALL2, 0);
            if (icon == IntPtr.Zero)
                icon = GetClassLongPtr(hWnd, GCLP_HICON);
            return icon;
        }

         private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")] private static extern IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex);
    }
}
