using System;
using System.Runtime.InteropServices;

namespace PinyinSwitcher.Services
{
    internal sealed class HotkeyService
    {
        private const int HotkeyId = 0x5053;
        private const uint ModAlt = 0x0001;
        private const uint ModControl = 0x0002;
        private const uint ModNoRepeat = 0x4000;
        private const uint KeyP = 0x50;
        private IntPtr _windowHandle;

        public bool Register(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            return RegisterHotKey(windowHandle, HotkeyId, ModControl | ModAlt | ModNoRepeat, KeyP);
        }

        public bool IsHotkeyMessage(int message, IntPtr wParam)
        {
            return message == 0x0312 && wParam.ToInt32() == HotkeyId;
        }

        public void Unregister()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HotkeyId);
                _windowHandle = IntPtr.Zero;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint virtualKey);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
