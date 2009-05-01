using System.Collections.Generic;

namespace TMSPS.Core.PluginSystem.Configuration
{
    public class PluginSettingsStore : Dictionary<ushort, PluginSettings>
    {
        public PluginSettings Get(ushort pluginID)
        {
            return !ContainsKey(pluginID) ? Reset(pluginID) : this[pluginID];
        }

        public PluginAreaSettings Get(ushort pluginID, byte areaID)
        {
            return !ContainsKey(pluginID) ? Reset(pluginID, areaID) : this[pluginID].AreaSettings.Get(areaID);
        }

        public PluginSettings Reset(ushort pluginID)
        {
            PluginSettings result = new PluginSettings();
            this[pluginID] = result;

            return result;
        }

        public PluginAreaSettings Reset(ushort pluginID, byte areaID)
        {
            return Reset(pluginID).AreaSettings.Get(areaID);
        }
    }
}