using System;
using System.Collections.Generic;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.SQLite
{
    public class RankingAdapter : BaseAdapter, IRankingAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RankingAdapter"/> class.
        /// </summary>
        public RankingAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RankingAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        public RankingAdapter(ConnectionManager connectionManager)
            : base(connectionManager)
        {
        }

        #endregion

        #region IRankingAdapter Members

        public Ranking Deserialize_ByLogin(string login)
        {
            throw new NotImplementedException();
        }

        public Ranking Deserialize_ByRank(int rank)
        {
            throw new NotImplementedException();
        }

        public Ranking GetNextRank(string login)
        {
            throw new NotImplementedException();
        }

        public List<Ranking> Deserialize_List(uint top)
        {
            throw new NotImplementedException();
        }

        public List<RankingStats> DeserializeListByMost(uint top, uint rankLimit)
        {
            throw new NotImplementedException();
        }

        public void ReCreateAll()
        {
            throw new NotImplementedException();
        }

        public uint GetTopRankingsCount()
        {
            throw new NotImplementedException();
        }

        public List<TopRankingEntry> GetTopRankings(uint startIndex, uint endIndex)
        {
            throw new NotImplementedException();
        }

        public void UpdateForChallenge(int challengeID)
        {
            throw new NotImplementedException();
        }

        public void UpdateForChallenge(string uniqueChallengeID)
        {
            throw new NotImplementedException();
        }

        public List<Ranking> Deserialize_PagedList(uint startIndex, uint endIndex)
        {
            throw new NotImplementedException();
        }

        public uint GetRanksCount()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
