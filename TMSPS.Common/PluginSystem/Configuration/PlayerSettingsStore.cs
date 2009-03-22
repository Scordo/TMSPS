using System.Collections.Generic;

namespace TMSPS.Core.PluginSystem.Configuration
{
    public class PlayerSettingsStore : Dictionary<string, PlayerSettings>
    {
        public PlayerSettings Get(string login)
        {
            return !ContainsKey(login.ToLower()) ? Reset(login) : this[login.ToLower()];
        }

        public PluginSettings Get(string login, ushort pluginID)
        {
            return !ContainsKey(login) ? Reset(login, pluginID) : Get(login).PluginSettings.Get(pluginID);
        }

        public PluginAreaSettings Get(string login, ushort pluginID, byte areaID)
        {
            return !ContainsKey(login) ? Reset(login, pluginID, areaID) : Get(login, pluginID).AreaSettings.Get(areaID);
        }

        public PlayerSettings Reset(string login)
        {
            PlayerSettings result = new PlayerSettings();
            this[login.ToLower()] = result;

            return result;
        }

        public PluginSettings Reset(string login, ushort pluginID)
        {
            return Reset(login).PluginSettings.Get(pluginID);
        }

        public PluginAreaSettings Reset(string login, ushort pluginID, byte areaID)
        {
            return Reset(login, pluginID).AreaSettings.Get(areaID);
        }
    }
}