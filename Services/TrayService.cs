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

        public TrayService(
            Action<PinyinMode> setMode,
            Action toggle,
            Action showSettings,
            Action<bool> setStartup,
            Action exit)
        {
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
                Visible = true
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
            _notifyIcon.Text = available
                ? "Pinyin Switcher - 当前：" + GetDisplayName(mode.Value)
                : "Pinyin Switcher - 未检测到微软拼音";
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
    }
}
