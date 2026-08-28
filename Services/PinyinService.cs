using System;
using System.Globalization;
using Microsoft.Win32;
using PinyinSwitcher.Models;

namespace PinyinSwitcher.Services
{
    public sealed class PinyinService
    {
        internal const string RegistryPath = @"Software\Microsoft\InputMethod\Settings\CHS";
        private const string RegistryValueName = "Enable Double Pinyin";
        private readonly ImeRefreshService _imeRefreshService = new ImeRefreshService();

        public bool IsMicrosoftPinyinInstalled()
        {
            using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            using (RegistryKey key = root.OpenSubKey(RegistryPath, false))
            {
                return key != null && key.GetValue(RegistryValueName) != null;
            }
        }

        public PinyinMode GetCurrentMode()
        {
            using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            using (RegistryKey key = root.OpenSubKey(RegistryPath, false))
            {
                object value = GetRequiredValue(key);

                try
                {
                    return ParseMode(Convert.ToInt32(value, CultureInfo.InvariantCulture));
                }
                catch (FormatException exception)
                {
                    throw new InvalidOperationException("微软拼音模式值不是有效数字。", exception);
                }
                catch (OverflowException exception)
                {
                    throw new InvalidOperationException("微软拼音模式值超出有效范围。", exception);
                }
            }
        }

        public bool SetMode(PinyinMode mode)
        {
            if (GetCurrentMode() == mode)
            {
                return true;
            }

            using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            using (RegistryKey key = root.OpenSubKey(RegistryPath, true))
            {
                GetRequiredValue(key);
                key.SetValue(RegistryValueName, (int)mode, RegistryValueKind.DWord);
            }

            return _imeRefreshService.Refresh(RegistryPath);
        }

        public bool Toggle(out PinyinMode mode)
        {
            mode = GetCurrentMode() == PinyinMode.FullPinyin
                ? PinyinMode.DoublePinyin
                : PinyinMode.FullPinyin;
            return SetMode(mode);
        }

        internal static PinyinMode ParseMode(int value)
        {
            if (value == 0)
            {
                return PinyinMode.FullPinyin;
            }

            if (value == 1)
            {
                return PinyinMode.DoublePinyin;
            }

            throw new InvalidOperationException("未知的微软拼音模式值：" + value);
        }

        private static object GetRequiredValue(RegistryKey key)
        {
            object value = key == null ? null : key.GetValue(RegistryValueName);
            if (value == null)
            {
                throw new InvalidOperationException("未检测到 Microsoft Pinyin 的模式配置。请先在系统设置中启用微软拼音。");
            }

            return value;
        }
    }
}
