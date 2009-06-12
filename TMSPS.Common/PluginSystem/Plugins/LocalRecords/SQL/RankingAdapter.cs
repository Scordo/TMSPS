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

        public Ranking Deserialize_ByRank(int rank)
        {
            return SqlHelper.ExecuteClassQuery<Ranking>("Ranking_Deserialize_ByRank", RankingFromDataRow, "rank", rank);
        }

        public Ranking GetNextRank(string login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            return SqlHelper.ExecuteClassQuery<Ranking>("Ranking_GetNextRank", RankingFromDataRow, "login", login);
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

        public uint GetTopRankingsCount()
        {
            return Convert.ToUInt32(SqlHelper.ExecuteScalar<int>("Ranking_GetTopRankingsCount"));
        }

        public List<TopRankingEntry> GetTopRankings(uint startIndex, uint endIndex)
        {
            return SqlHelper.ExecuteClassListQuery<TopRankingEntry>("Ranking_GetTopRankings", TopRankingEntryDataRow, "startIndex", (int)startIndex, "endIndex", (int)endIndex);            
        }

        public void UpdateForChallenge(int challengeID)
        {
            SqlHelper.ExecuteNonQuery("Ranking_UpdateForChallenge", "ChallengeID", challengeID);
        }

        public void UpdateForChallenge(string uniqueChallengeID)
        {
            SqlHelper.ExecuteNonQuery("Ranking_UpdateForChallenge_ByChallengeUID", "UniqueID", uniqueChallengeID);
        }

        #endregion

        #region Non Public Methods

        private static Ranking RankingFromDataRow(DataRow row)
        {
            string nickname = Convert.ToString(row["Nickname"]);
            string login = Convert.ToString(row["Login"]);
            ushort rank = Convert.ToUInt16(row["Rank"]);
            int playerID = Convert.ToInt32(row["PlayerID"]);
            double averageRank = Convert.ToDouble(row["AverageRank"]);
            uint recordsCount = Convert.ToUInt32(row["RecordsCount"]);
            uint challengesCount = Convert.ToUInt32(row["ChallengesCount"]);
            double score = Convert.ToDouble(row["Score"]);

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

        private static TopRankingEntry TopRankingEntryDataRow(DataRow row)
        {
            return new TopRankingEntry
            {
                Position = Convert.ToUInt32(row["Position"]),
                Login = Convert.ToString(row["Login"]),
                Nickname = Convert.ToString(row["Nickname"]),
                FirstRecords = Convert.ToUInt32(row["FirstRecords"]),
                SecondRecords = Convert.ToUInt32(row["SecondRecords"]),
                ThirdRecords = Convert.ToUInt32(row["ThirdRecords"])
            };
               
        }

        #endregion
    }
}
