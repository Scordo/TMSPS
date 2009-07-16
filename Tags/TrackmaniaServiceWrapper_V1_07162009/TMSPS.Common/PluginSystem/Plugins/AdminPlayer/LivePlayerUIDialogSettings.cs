using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.Core.PluginSystem.Plugins.AdminPlayer
{
    public class LivePlayerUIDialogSettings : PagedUIDialogSettingsBase<LivePlayerUIDialogSettings>
    {
        public string IgnoreText { get; private set; }
        public string UnIgnoreText { get; private set; }
        public string BanText { get; private set; }
        public string UnBanText { get; private set; }
        public string AddToBlackListText { get; private set; }
        public string RemoveFromBlackListText { get; private set; }
        public string AddGuestText { get; private set; }
        public string RemoveGuestText { get; private set; }
        public string SpectatorText { get; private set; }
        public string ForceSpectatorText { get; private set; }

        public new static LivePlayerUIDialogSettings ReadFromFile(string xmlConfigurationFile)
        {
            return ReadFromFile(xmlConfigurationFile, (rootElement, result) =>
            {
                result.AddGuestText = ReadConfigString(rootElement, "AddGuestText", xmlConfigurationFile);
                result.IgnoreText = ReadConfigString(rootElement, "IgnoreText", xmlConfigurationFile);
                result.UnIgnoreText = ReadConfigString(rootElement, "UnIgnoreText", xmlConfigurationFile);
                result.BanText = ReadConfigString(rootElement, "BanText", xmlConfigurationFile);
                result.UnBanText = ReadConfigString(rootElement, "UnBanText", xmlConfigurationFile);
                result.AddToBlackListText = ReadConfigString(rootElement, "AddToBlackListText", xmlConfigurationFile);
                result.RemoveFromBlackListText = ReadConfigString(rootElement, "RemoveFromBlackListText", xmlConfigurationFile);
                result.AddGuestText = ReadConfigString(rootElement, "AddGuestText", xmlConfigurationFile);
                result.RemoveGuestText = ReadConfigString(rootElement, "RemoveGuestText", xmlConfigurationFile);
                result.SpectatorText = ReadConfigString(rootElement, "SpectatorText", xmlConfigurationFile);
                result.ForceSpectatorText = ReadConfigString(rootElement, "ForceSpectatorText", xmlConfigurationFile);
            });
        }
    }
}