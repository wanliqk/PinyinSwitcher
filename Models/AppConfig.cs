namespace PinyinSwitcher.Models
{
    public sealed class AppConfig
    {
        public string Hotkey { get; set; } = "Ctrl+Alt+P";

        public bool ShowNotification { get; set; } = true;

        public bool StartMinimized { get; set; } = true;
    }
}
