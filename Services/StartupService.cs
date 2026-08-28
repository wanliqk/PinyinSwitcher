using System;
using System.Reflection;
using Microsoft.Win32;

namespace PinyinSwitcher.Services
{
    internal sealed class StartupService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "PinyinSwitcher";

        public bool IsEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
            {
                string command = key == null ? null : key.GetValue(ValueName) as string;
                return string.Equals(command, GetCommand(), StringComparison.OrdinalIgnoreCase);
            }
        }

        public void SetEnabled(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
            {
                if (enabled)
                {
                    key.SetValue(ValueName, GetCommand(), RegistryValueKind.String);
                }
                else
                {
                    key.DeleteValue(ValueName, false);
                }
            }
        }

        private static string GetCommand()
        {
            return string.Concat((char)34, Assembly.GetExecutingAssembly().Location, (char)34);
        }
    }
}
