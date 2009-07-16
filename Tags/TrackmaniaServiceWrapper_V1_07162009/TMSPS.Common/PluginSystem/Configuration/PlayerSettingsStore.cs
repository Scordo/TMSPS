using System;
using System.Collections.Generic;
using System.Linq;

namespace TMSPS.Core.PluginSystem.Configuration
{
    public class PlayerSettingsStore : Dictionary<string, PlayerSettings>
    {
        private readonly object _dictionaryAccessLog = new object();

        public PlayerSettings Get(string login)
        {
            return Get(login, false);
        }

        public PlayerSettings Get(string login, bool createOnDemand)
        {
            if (ContainsKey(login.ToLower()))
                return this[login.ToLower()];

            return createOnDemand ? Reset(login) : null;
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

            lock (_dictionaryAccessLog)
            {
                this[login.ToLower()] = result;
            }

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

        public List<PlayerSettings> GetAllAsList()
        {
            lock (_dictionaryAccessLog)
            {
                return this.Select(x => x.Value).ToList();
            }
        }

        public List<PlayerSettings> GetAsList(Predicate<PlayerSettings> predicate)
        {
            lock (_dictionaryAccessLog)
            {
                return this.Where(x => predicate(x.Value)).Select(x => x.Value).ToList();
            }
        }
    }
}