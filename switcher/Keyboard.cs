using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace switcher
{
    public class Keyboard : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private IntPtr _hookHandle = IntPtr.Zero;
        private readonly LowLevelKeyboardProc _proc;

        public event Action? OnAltTabPressed;
        public event Action? OnAltReleased;

        private bool _altDown = false;
        private bool _switcherActive = false;

        public Keyboard()
        {
            _proc = HookCallback;
        }

        public void Install()
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        public void Uninstall()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
                {
                    if (key == Key.LeftAlt || key == Key.RightAlt)
                        _altDown = true;

                    if (key == Key.Tab && _altDown)
                    {
                        _switcherActive = true;
                        OnAltTabPressed?.Invoke();
                        return 1;
                    }
                }

                if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
                {
                    if (key == Key.LeftAlt || key == Key.RightAlt)
                    {
                        _altDown = false;
                        if (_switcherActive)
                        {
                            _switcherActive = false;
                            OnAltReleased?.Invoke();
                        }
                    }
                }
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        public void Dispose() => Uninstall();

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")] private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")] private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
