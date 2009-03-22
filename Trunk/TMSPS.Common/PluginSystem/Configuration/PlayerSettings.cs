namespace TMSPS.Core.PluginSystem.Configuration
{
    public class PlayerSettings
    {
        private readonly PluginSettingsStore _pluginSettings = new PluginSettingsStore();

        public PluginSettingsStore PluginSettings
        {
            get { return _pluginSettings; }
        }
    }
}
