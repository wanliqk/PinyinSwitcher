using System;
using System.Drawing;
using System.Windows.Forms;
using PinyinSwitcher.Models;

namespace PinyinSwitcher.Services
{
    internal sealed class TrayService : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _menu;
        private readonly ToolStripMenuItem _fullPinyinItem;
        private readonly ToolStripMenuItem _doublePinyinItem;
        private readonly ToolStripMenuItem _startupItem;
        private readonly Icon _fullIcon;
        private readonly Icon _doubleIcon;

        public TrayService(
            Action<PinyinMode> setMode,
            Action toggle,
            Action showSettings,
            Action<bool> setStartup,
            Action exit)
        {
            _fullIcon = LoadIcon("PinyinSwitcher.Resources.full.ico");
            _doubleIcon = LoadIcon("PinyinSwitcher.Resources.double.ico");
            _fullPinyinItem = new ToolStripMenuItem("全拼", null, (sender, args) => setMode(PinyinMode.FullPinyin));
            _doublePinyinItem = new ToolStripMenuItem("双拼", null, (sender, args) => setMode(PinyinMode.DoublePinyin));

            ToolStripMenuItem toggleItem = new ToolStripMenuItem("快速切换", null, (sender, args) => toggle())
            {
                ShortcutKeyDisplayString = "Ctrl + Alt + P"
            };
            ToolStripMenuItem settingsItem = new ToolStripMenuItem("设置", null, (sender, args) => showSettings());
            _startupItem = new ToolStripMenuItem("开机启动", null, (sender, args) => setStartup(!_startupItem.Checked));
            ToolStripMenuItem exitItem = new ToolStripMenuItem("退出", null, (sender, args) => exit());

            _menu = new ContextMenuStrip();
            _menu.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Pinyin Switcher") { Enabled = false },
                new ToolStripSeparator(),
                _fullPinyinItem,
                _doublePinyinItem,
                new ToolStripSeparator(),
                toggleItem,
                new ToolStripSeparator(),
                settingsItem,
                _startupItem,
                new ToolStripSeparator(),
                exitItem
            });

            _notifyIcon = new NotifyIcon
            {
                ContextMenuStrip = _menu,
                Icon = SystemIcons.Application,
                Text = "Pinyin Switcher",
                Visible = false
            };
            _notifyIcon.DoubleClick += (sender, args) => showSettings();
        }

        public void Update(PinyinMode? mode, bool startupEnabled)
        {
            bool available = mode.HasValue;
            _fullPinyinItem.Enabled = available;
            _doublePinyinItem.Enabled = available;
            _fullPinyinItem.Checked = mode == PinyinMode.FullPinyin;
            _doublePinyinItem.Checked = mode == PinyinMode.DoublePinyin;
            _startupItem.Checked = startupEnabled;
            _notifyIcon.Icon = available
                ? mode == PinyinMode.FullPinyin ? _fullIcon : _doubleIcon
                : SystemIcons.Application;
            _notifyIcon.Text = available
                ? "Pinyin Switcher - " + GetDisplayName(mode.Value)
                : "Pinyin Switcher - 未检测到微软拼音";
            _notifyIcon.Visible = true;
        }

        public void ShowInfo(string message)
        {
            ShowBalloon(message, ToolTipIcon.Info);
        }

        public void ShowError(string message)
        {
            ShowBalloon(message, ToolTipIcon.Warning);
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _menu.Dispose();
            _fullIcon.Dispose();
            _doubleIcon.Dispose();
        }

        private void ShowBalloon(string message, ToolTipIcon icon)
        {
            _notifyIcon.BalloonTipTitle = "Pinyin Switcher";
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.BalloonTipIcon = icon;
            _notifyIcon.ShowBalloonTip(3000);
        }

        private static string GetDisplayName(PinyinMode mode)
        {
            return mode == PinyinMode.FullPinyin ? "全拼" : "双拼";
        }

        private static Icon LoadIcon(string resourceName)
        {
            using (System.IO.Stream stream = typeof(TrayService).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("未找到托盘图标资源：" + resourceName);
                }

                using (Icon icon = new Icon(stream, SystemInformation.SmallIconSize))
                {
                    return (Icon)icon.Clone();
                }
            }
        }
    }
}
