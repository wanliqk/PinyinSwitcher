using System;
using System.IO;
using System.Text;

namespace PinyinSwitcher.Utils
{
    internal static class Logger
    {
        private static readonly object SyncRoot = new object();

        public static void Write(string message)
        {
            try
            {
                lock (SyncRoot)
                {
                    string directory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "PinyinSwitcher",
                        "logs");
                    Directory.CreateDirectory(directory);
                    File.AppendAllText(
                        Path.Combine(directory, DateTime.Now.ToString("yyyy-MM-dd") + ".log"),
                        DateTime.Now.ToString("HH:mm:ss") + " " + message + Environment.NewLine,
                        Encoding.UTF8);
                }
            }
            catch
            {
                // Logging must never crash the tray application.
            }
        }
    }
}
