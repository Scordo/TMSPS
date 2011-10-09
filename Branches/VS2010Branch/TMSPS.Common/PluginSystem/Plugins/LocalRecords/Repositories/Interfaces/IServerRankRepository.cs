using System.Collections.Generic;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces
{
    public interface IServerRankRepository : IRepositoryBase
    {
        uint GetRanksCount();
        Ranking Get(int playerId);
        Ranking GetNextRank(int playerId);
        List<Ranking> GetList(uint top);
        List<Ranking> GetList(uint startIndex, uint endIndex);
        void ReCreateRanks();
    }
}