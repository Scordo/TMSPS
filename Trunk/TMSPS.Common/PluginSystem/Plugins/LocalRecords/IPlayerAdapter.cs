using System.Collections.Generic;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface IPlayerAdapter : IBaseAdapter
    {
        void CreateOrUpdate(Player player);
        uint IncreaseWins(string login);
        ulong? UpdateTimePlayed(string login);
        List<Player> DeserializeList(uint top, PlayerSortOrder sorting, bool ascending);
    }

    public enum PlayerSortOrder
    {
        Wins = 0,
        TimePlayed = 1,
    }
}