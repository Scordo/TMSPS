using System.Collections.Generic;

namespace TMSPS.Core.PluginSystem.Configuration
{
    public class PluginAreaSettingsStore : Dictionary<byte, PluginAreaSettings>
    {
        public PluginAreaSettings Get(byte areaID)
        {
            return !ContainsKey(areaID) ? Reset(areaID) : this[areaID];
        }

        public PluginAreaSettings Reset(byte areaID)
        {
            PluginAreaSettings result = new PluginAreaSettings();
            this[areaID] = result;

            return result;
        }
    }
}