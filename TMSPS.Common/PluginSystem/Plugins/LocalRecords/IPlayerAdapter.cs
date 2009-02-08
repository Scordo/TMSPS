using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface IPlayerAdapter : IBaseAdapter
    {
        void CreateOrUpdate(Player player);
        uint IncreaseWins(string login);
        ulong UpdateTimePlayed(string login);
    }
}