using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface IRatingAdapter : IBaseAdapter
    {
        double? Vote(string login, int challengeID, ushort rating);
    }
}