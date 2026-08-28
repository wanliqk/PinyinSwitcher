using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using PinyinSwitcher.Models;
using PinyinSwitcher.Utils;

namespace PinyinSwitcher.Services
{
    internal sealed class ConfigService
    {
        private readonly string _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PinyinSwitcher",
            "config.json");

        public AppConfig Load()
        {
            if (!File.Exists(_configPath))
            {
                return new AppConfig();
            }

            try
            {
                return new JavaScriptSerializer().Deserialize<AppConfig>(
                    File.ReadAllText(_configPath, Encoding.UTF8)) ?? new AppConfig();
            }
            catch (Exception exception)
            {
                Logger.Write("Config load failed: " + exception);
                return new AppConfig();
            }
        }

        public void Save(AppConfig config)
        {
            string directory = Path.GetDirectoryName(_configPath);
            string temporaryPath = _configPath + ".tmp";
            Directory.CreateDirectory(directory);

            try
            {
                File.WriteAllText(
                    temporaryPath,
                    new JavaScriptSerializer().Serialize(config),
                    new UTF8Encoding(false));

                if (File.Exists(_configPath))
                {
                    File.Replace(temporaryPath, _configPath, null);
                }
                else
                {
                    File.Move(temporaryPath, _configPath);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
    }
}
