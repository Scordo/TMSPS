using System;
using System.Collections.Generic;
using System.Data;
using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL
{
    public class RankingAdapter : SQLBaseAdapter, IRankingAdapter
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
        public RankingAdapter(ConnectionManager connectionManager): base(connectionManager)
        {
        }

        #endregion

        #region Public Methods

        public Ranking Deserialize_ByLogin(string login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            return SqlHelper.ExecuteClassQuery<Ranking>("Ranking_Deserialize_ByLogin", RankingFromDataRow, "login", login);
        }

        public List<Ranking> Deserialize_List(uint top)
        {
            return SqlHelper.ExecuteClassListQuery<Ranking>("Ranking_Deserialize_List", RankingFromDataRow, "amountOfRankings", (int) top);
        }

        public List<RankingStats> DeserializeListByMost(uint top, uint rankLimit)
        {
            return SqlHelper.ExecuteClassListQuery<RankingStats>("Ranking_Deserialize_List_ByMost", RankingStatsFromDataRow, "top", (int)top, "rankLimit", (int)rankLimit);
        }

        public void ReCreateAll()
        {
            SqlHelper.ExecuteNonQuery("Ranking_ReCreateAll");
        }

        #endregion

        #region Non Public Methods

        private static Ranking RankingFromDataRow(DataRow row)
        {
            string nickname = Convert.ToString(row["Nickname"]);
            string login = Convert.ToString(row["Login"]);
            ushort rank = Convert.ToUInt16(row["Rank"]);
            int playerID = Convert.ToInt32(row["PlayerID"]);
            double averageRank = Convert.ToInt32(row["AverageRank"]);
            uint recordsCount = Convert.ToUInt32(row["RecordsCount"]);
            uint challengesCount = Convert.ToUInt32(row["ChallengesCount"]);
            double score = Convert.ToUInt32(row["Score"]);

            return new Ranking
            {
               AverageRank = averageRank,
               ChallengesCount = challengesCount,
               CurrentRank = rank,
               Login = login,
               Nickname = nickname,
               PlayerID = playerID,
               RecordsCount = recordsCount,
               Score = score
            };
        }

        private static RankingStats RankingStatsFromDataRow(DataRow row)
        {
            string nickname = Convert.ToString(row["Nickname"]);
            int playerID = Convert.ToInt32(row["PlayerID"]);
            uint ranksCount = Convert.ToUInt32(row["RanksCount"]);

            return new RankingStats(nickname, playerID, ranksCount);
        }

        #endregion
    }
}