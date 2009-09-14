using TMSPS.Core.PluginSystem.Configuration;


namespace TMSPS.Core.PluginSystem.Plugins.Core
{
    public class HelpSettings : PagedUIDialogSettingsBase<HelpSettings>
    {
        public string HelpButtonTemplate { get; private set; }

        public new static HelpSettings ReadFromFile(string xmlConfigurationFile)
        {
            return ReadFromFile(xmlConfigurationFile, (rootElement, result) =>
            {
                // nothing here
            });
        }
    }
}