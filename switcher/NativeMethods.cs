using System.Runtime.InteropServices;

namespace switcher
{
    public static class NativeMethods
    {
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
