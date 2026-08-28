using System;
using System.Runtime.InteropServices;

namespace PinyinSwitcher.Services
{
    public sealed class ImeRefreshService
    {
        private static readonly IntPtr HwndBroadcast = new IntPtr(0xffff);
        private const uint WmSettingChange = 0x001a;
        private const uint SmtoAbortIfHung = 0x0002;

        public bool Refresh(string settingArea)
        {
            UIntPtr result;
            return SendMessageTimeout(
                HwndBroadcast,
                WmSettingChange,
                UIntPtr.Zero,
                settingArea,
                SmtoAbortIfHung,
                2000,
                out result) != IntPtr.Zero;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint message,
            UIntPtr wParam,
            string lParam,
            uint flags,
            uint timeout,
            out UIntPtr result);
    }
}
