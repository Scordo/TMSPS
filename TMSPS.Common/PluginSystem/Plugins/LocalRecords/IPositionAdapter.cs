using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface IPositionAdapter : IBaseAdapter
    {
        void AddPosition(string login, string uniqueChallengeID, ushort position, ushort maxPosition);
    }
}