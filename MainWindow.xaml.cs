using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using PinyinSwitcher.Models;
using PinyinSwitcher.Services;
using PinyinSwitcher.Utils;

namespace PinyinSwitcher
{
    public partial class MainWindow : Window
    {
        private readonly ConfigService _configService = new ConfigService();
        private readonly PinyinService _pinyinService = new PinyinService();
        private readonly StartupService _startupService = new StartupService();
        private readonly HotkeyService _hotkeyService = new HotkeyService();
        private readonly AppConfig _config;
        private readonly TrayService _trayService;
        private HwndSource _messageSource;
        private bool _allowClose;
        private bool _missingPinyinNotified;

        public MainWindow()
        {
            InitializeComponent();
            _config = _configService.Load();
            _trayService = new TrayService(SetMode, ToggleMode, ShowSettings, SetStartup, ExitApplication);
            InitializeBackgroundServices();

            if (!_config.StartMinimized)
            {
                Dispatcher.BeginInvoke(new Action(ShowSettings));
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _hotkeyService.Unregister();
            if (_messageSource != null)
            {
                _messageSource.RemoveHook(WindowMessageHook);
            }

            _trayService.Dispose();
            base.OnClosed(e);
        }

        private void InitializeBackgroundServices()
        {
            IntPtr handle = new WindowInteropHelper(this).EnsureHandle();
            _messageSource = HwndSource.FromHwnd(handle);
            _messageSource.AddHook(WindowMessageHook);

            if (!_hotkeyService.Register(handle))
            {
                Logger.Write("Global hotkey registration failed");
                _trayService.ShowError("Ctrl + Alt + P 注册失败，快捷键可能已被占用。");
            }

            RefreshState();
        }

        private IntPtr WindowMessageHook(
            IntPtr windowHandle,
            int message,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (_hotkeyService.IsHotkeyMessage(message, wParam))
            {
                handled = true;
                ToggleMode();
            }

            return IntPtr.Zero;
        }

        private void ShowSettings()
        {
            RefreshState();
            ShowNotificationCheckBox.IsChecked = _config.ShowNotification;
            StartMinimizedCheckBox.IsChecked = _config.StartMinimized;
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void RefreshState()
        {
            bool startupEnabled = false;
            try
            {
                startupEnabled = _startupService.IsEnabled();
                StartWithWindowsCheckBox.IsChecked = startupEnabled;

                if (!_pinyinService.IsMicrosoftPinyinInstalled())
                {
                    SetModeControls(null);
                    _trayService.Update(null, startupEnabled);
                    if (!_missingPinyinNotified)
                    {
                        _missingPinyinNotified = true;
                        _trayService.ShowError("未检测到 Microsoft Pinyin。");
                    }
                    return;
                }

                _missingPinyinNotified = false;
                PinyinMode mode = _pinyinService.GetCurrentMode();
                SetModeControls(mode);
                _trayService.Update(mode, startupEnabled);
            }
            catch (Exception exception)
            {
                Logger.Write("State refresh failed: " + exception);
                SetModeControls(null);
                _trayService.Update(null, startupEnabled);
                _trayService.ShowError(exception.Message);
            }
        }

        private void SetModeControls(PinyinMode? mode)
        {
            bool available = mode.HasValue;
            FullPinyinRadioButton.IsEnabled = available;
            DoublePinyinRadioButton.IsEnabled = available;
            FullPinyinRadioButton.IsChecked = mode == PinyinMode.FullPinyin;
            DoublePinyinRadioButton.IsChecked = mode == PinyinMode.DoublePinyin;
            CurrentModeTextBlock.Text = available ? "当前：" + GetDisplayName(mode.Value) : "未检测到微软拼音";
        }

        private void ToggleMode()
        {
            try
            {
                PinyinMode target = _pinyinService.GetCurrentMode() == PinyinMode.FullPinyin
                    ? PinyinMode.DoublePinyin
                    : PinyinMode.FullPinyin;
                SetMode(target);
            }
            catch (Exception exception)
            {
                HandleError("切换失败", exception);
            }
        }

        private void SetMode(PinyinMode mode)
        {
            try
            {
                bool refreshSucceeded = _pinyinService.SetMode(mode);
                RefreshState();
                Logger.Write("Mode changed to " + mode);

                if (!refreshSucceeded)
                {
                    _trayService.ShowError("注册表已更新，但 IME 刷新广播失败或超时。");
                }
                else if (_config.ShowNotification)
                {
                    _trayService.ShowInfo("已切换至：" + GetDisplayName(mode));
                }
            }
            catch (Exception exception)
            {
                HandleError("切换失败", exception);
            }
        }

        private void SetStartup(bool enabled)
        {
            try
            {
                _startupService.SetEnabled(enabled);
                RefreshState();
            }
            catch (Exception exception)
            {
                HandleError("开机启动设置失败", exception);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _config.ShowNotification = ShowNotificationCheckBox.IsChecked == true;
                _config.StartMinimized = StartMinimizedCheckBox.IsChecked == true;
                _configService.Save(_config);
                _startupService.SetEnabled(StartWithWindowsCheckBox.IsChecked == true);

                if (FullPinyinRadioButton.IsChecked == true)
                {
                    SetMode(PinyinMode.FullPinyin);
                }
                else if (DoublePinyinRadioButton.IsChecked == true)
                {
                    SetMode(PinyinMode.DoublePinyin);
                }

                RefreshState();
                Hide();
            }
            catch (Exception exception)
            {
                HandleError("保存设置失败", exception);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ExitApplication()
        {
            _allowClose = true;
            Close();
            Application.Current.Shutdown();
        }

        private void HandleError(string operation, Exception exception)
        {
            Logger.Write(operation + ": " + exception);
            RefreshState();
            _trayService.ShowError(operation + "：" + exception.Message);
        }

        private static string GetDisplayName(PinyinMode mode)
        {
            return mode == PinyinMode.FullPinyin ? "全拼" : "双拼";
        }
    }
}
