using System.Collections.Generic;

namespace TMSPS.Core.PluginSystem.Configuration
{
    public class ManiaLinkPageHashStore : Dictionary<string, string>
    {
        public string Get(string maniaLinkPageID)
        {
            return !ContainsKey(maniaLinkPageID) ? null : this[maniaLinkPageID];
        }
    }
}