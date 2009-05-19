namespace TMSPS.Core.PluginSystem.Configuration
{
    public class PlayerSettings
    {
        private readonly PluginSettingsStore _pluginSettings = new PluginSettingsStore();
        private readonly ManiaLinkPageHashStore _maniaLinkPageHashStore = new ManiaLinkPageHashStore();

        public PluginSettingsStore PluginSettings
        {
            get { return _pluginSettings; }
        }

        public ManiaLinkPageHashStore ManiaLinkPageHashStore
        {
            get { return _maniaLinkPageHashStore; }
        }
    }
}