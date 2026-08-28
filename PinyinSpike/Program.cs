using System;
using System.Text;
using System.Web.Script.Serialization;
using PinyinSwitcher.Models;
using PinyinSwitcher.Services;
using PinyinSwitcher.Tools;

namespace PinyinSpike
{
    internal static class Program
    {
        private static readonly PinyinService PinyinService = new PinyinService();
        private static readonly ImeRefreshService ImeRefreshService = new ImeRefreshService();

        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length == 1 && string.Equals(args[0], "--self-test", StringComparison.OrdinalIgnoreCase))
            {
                return RunSelfTest();
            }

            if (args.Length == 1 && string.Equals(args[0], "--generate-tray-icons", StringComparison.OrdinalIgnoreCase))
            {
                return GenerateTrayIcons();
            }

            while (true)
            {
                PrintMenu();
                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            Console.WriteLine("当前：{0}\n", GetDisplayName(PinyinService.GetCurrentMode()));
                            break;
                        case "2":
                            SetMode(PinyinMode.FullPinyin);
                            break;
                        case "3":
                            SetMode(PinyinMode.DoublePinyin);
                            break;
                        case "4":
                            PrintRefreshResult(ImeRefreshService.Refresh(PinyinService.RegistryPath));
                            break;
                        case "5":
                            return 0;
                        default:
                            Console.WriteLine("请输入 1-5。\n");
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("操作失败：{0}\n", exception.Message);
                }
            }
        }

        private static void PrintMenu()
        {
            Console.WriteLine("PinyinSpike");
            Console.WriteLine("1. 当前状态");
            Console.WriteLine("2. 切换全拼");
            Console.WriteLine("3. 切换双拼");
            Console.WriteLine("4. 刷新 IME");
            Console.WriteLine("5. 退出");
            Console.Write("请选择：");
        }

        private static void SetMode(PinyinMode mode)
        {
            bool refreshSucceeded = PinyinService.SetMode(mode);
            PrintRefreshResult(refreshSucceeded);
            Console.WriteLine("注册表当前状态：{0}", GetDisplayName(PinyinService.GetCurrentMode()));
            Console.WriteLine("请立即在已打开的输入框中验证微软拼音是否同步切换。\n");
        }

        private static void PrintRefreshResult(bool success)
        {
            Console.WriteLine(success
                ? "已广播 WM_SETTINGCHANGE。"
                : "WM_SETTINGCHANGE 广播失败或超时；注册表可能已修改，但 IME 未必立即刷新。");
        }

        private static int RunSelfTest()
        {
            if (PinyinService.ParseMode(0) != PinyinMode.FullPinyin ||
                PinyinService.ParseMode(1) != PinyinMode.DoublePinyin)
            {
                Console.Error.WriteLine("Self-test failed.");
                return 1;
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            AppConfig config = serializer.Deserialize<AppConfig>(serializer.Serialize(new AppConfig()));
            if (config == null || !config.ShowNotification || !config.StartMinimized || config.Hotkey != "Ctrl+Alt+P")
            {
                Console.Error.WriteLine("Self-test failed.");
                return 1;
            }

            try
            {
                PinyinService.ParseMode(2);
                Console.Error.WriteLine("Self-test failed.");
                return 1;
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Self-test passed.");
                return 0;
            }
        }

        private static int GenerateTrayIcons()
        {
            try
            {
                TrayIconGenerator.GenerateIcons();
                Console.WriteLine("已生成 Resources/full.ico 和 Resources/double.ico。");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("托盘图标生成失败：{0}", exception.Message);
                return 1;
            }
        }

        private static string GetDisplayName(PinyinMode mode)
        {
            return mode == PinyinMode.FullPinyin ? "全拼" : "双拼";
        }
    }
}
