using System.Collections.Generic;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface IRankingAdapter : IBaseAdapter
    {
        Ranking Deserialize_ByLogin(string login);
        Ranking Deserialize_ByRank(int rank);
        Ranking GetNextRank(string login);
        List<Ranking> Deserialize_List(uint top);
        List<RankingStats> DeserializeListByMost(uint top, uint rankLimit);
        void ReCreateAll();
        uint GetTopRankingsCount();
        List<TopRankingEntry> GetTopRankings(uint startIndex, uint endIndex);
        void UpdateForChallenge(int challengeID);
        void UpdateForChallenge(string uniqueChallengeID);
        List<Ranking> Deserialize_PagedList(uint startIndex, uint endIndex);
        uint GetRanksCount();
    }
}
