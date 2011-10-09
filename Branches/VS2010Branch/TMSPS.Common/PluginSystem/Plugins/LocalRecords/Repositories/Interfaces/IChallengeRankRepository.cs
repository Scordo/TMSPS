using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces
{
    public interface IChallengeRankRepository : IRepositoryBase
    {
        int? GetChallengeRank(int challengeId, int playerId);
        void RecreateForChallenge(int challengeId);
        PagedList<TopRankingEntry> GetChallengeRankLeadersHavingTop3Ranks(uint startIndex, uint endIndex);
        uint GetChallengeRankLeadersHavingTop3RanksCount();
        List<RankingStats> GetChallengeRankLeadersHavingTopXRanks(uint top, uint rankLimit);
    }
}