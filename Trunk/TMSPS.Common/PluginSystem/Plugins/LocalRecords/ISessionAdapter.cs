using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface ISessionAdapter : IBaseAdapter
    {
        void AddSession(string login, int challengeID, int timeOrScore);
    }
}