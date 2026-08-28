using System;
using System.Threading;
using System.Windows;
using PinyinSwitcher.Utils;

namespace PinyinSwitcher
{
    public partial class App : Application
    {
        private Mutex _singleInstanceMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool isFirstInstance;
            _singleInstanceMutex = new Mutex(true, @"Local\PinyinSwitcher.SingleInstance", out isFirstInstance);
            if (!isFirstInstance)
            {
                _singleInstanceMutex.Dispose();
                _singleInstanceMutex = null;
                Shutdown();
                return;
            }

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            try
            {
                MainWindow = new MainWindow();
                Logger.Write("App started");
            }
            catch (Exception exception)
            {
                Logger.Write("App startup failed: " + exception);
                MessageBox.Show(
                    "Pinyin Switcher 启动失败：" + exception.Message,
                    "Pinyin Switcher",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_singleInstanceMutex != null)
            {
                _singleInstanceMutex.ReleaseMutex();
                _singleInstanceMutex.Dispose();
            }

            Logger.Write("App exited");
            base.OnExit(e);
        }
    }
}
