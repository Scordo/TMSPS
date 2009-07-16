namespace TMSPS.Core.PluginSystem.Configuration
{
    public class PluginSettings
    {
        private readonly PluginAreaSettingsStore _areaSettings = new PluginAreaSettingsStore();

        public PluginAreaSettingsStore AreaSettings
        {
            get { return _areaSettings; }
        }

        public PluginAreaSettings GetAreaSettings(byte areaID)
        {
            return AreaSettings.Get(areaID);
        }
    }
}
