using System.Collections.Generic;
using System.Linq;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaTime
    {
        public string Login { get; set; }
        public int Best { get; set; }
        public int[] Checks { get; set; }

        public DedimaniaTime()
        {
            
        }

        public DedimaniaTime(string login, int bestTime, IEnumerable<int> checkPoints)
        {
            Login = login;
            Best = bestTime;
            Checks = checkPoints.ToArray();
        }
    }
}