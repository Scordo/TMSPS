using System;
using System.Collections.Generic;
using System.Data;
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
        public RankingAdapter(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        #endregion

        #region IRankingAdapter Members

        public List<TopRankingEntry> GetTopRankings(uint startIndex, uint endIndex)
        {
            //hard to implement, will be done later
            return new List<TopRankingEntry>();
        }

        public Ranking GetNextRank(string login)
        {
            int? nextRankNumber = GetNextRankNumber(login);

            if (!nextRankNumber.HasValue)
                nextRankNumber = GetMaxRankNumber();

           return nextRankNumber.HasValue ? Deserialize_ByRank(nextRankNumber.Value) : null;
        }

        public List<Ranking> Deserialize_List(uint top)
        {
            const string selectStatement = "Select P.Nickname, P.Login, R.* FROM Rank R INNER JOIN Player P on P.ID = R.PlayerID Order by Rank Asc LIMIT @amountOfRankings";
            return SqlHelper.ExecuteClassListQuery<Ranking>(selectStatement, RankingFromDataRow, "amountOfRankings", (int)top);
        }

        public List<RankingStats> DeserializeListByMost(uint top, uint rankLimit)
        {
            const string selectStatement = "Select P.Nickname, M.PlayerID, M.RanksCount " +
                                           "FROM (Select PlayerID, Count([Rank]) as RanksCount FROM Ranking WHERE [Rank] <= @rankLimit Group by PlayerID Order by RanksCount desc LIMIT @top) M " +
                                           "INNER JOIN Player P on P.Id = M.PlayerID";


            return SqlHelper.ExecuteClassListQuery<RankingStats>(selectStatement, RankingStatsFromDataRow, "top", (int)top, "rankLimit", (int)rankLimit);
        }

        public void ReCreateAll()
        {
            const string selectStatement = "Select ID From Challenge";
            List<int> challengeIDs = SqlHelper.ExecuteClassListQuery(selectStatement, (DataRow r) => Convert.ToInt32(r["ID"]));

            challengeIDs.ForEach(UpdateForChallenge);
        }

        public void UpdateForChallenge(int challengeID)
        {
            const string deleteStatement = "Delete FROM Ranking WHERE ChallengeID = @ChallengeID";
            SqlHelper.ExecuteNonQuery(deleteStatement, "ChallengeID", challengeID);

            const string insertStatement = "INSERT INTO Ranking Select RowID as Rank, @ChallengeID as ChallengeID, PlayerID From Record WHERE ChallengeID = @ChallengeID order by TimeOrScore asc, LastChanged asc";
            SqlHelper.ExecuteNonQuery(insertStatement, "ChallengeID", challengeID);

            ReCreateRankTable();
        }

        public void UpdateForChallenge(string uniqueChallengeID)
        {
            const string selectStatement = "Select ID From [Challenge] where [UniqueID] = @UniqueID";
            int? challengeID = SqlHelper.ExecuteScalar<int?>(selectStatement, "UniqueID", uniqueChallengeID);

            if (!challengeID.HasValue)
                return;

            UpdateForChallenge(challengeID.Value);
        }

        public List<Ranking> Deserialize_PagedList(uint startIndex, uint endIndex)
        {
            const string selectStatement = "Select P.Nickname, P.Login,	R.* FROM Rank R INNER JOIN Player P on P.ID = R.PlayerID WHERE R.Rank between @startIndex + 1  and @endIndex + 1 Order by Rank Asc";
            return SqlHelper.ExecuteClassListQuery<Ranking>(selectStatement, RankingFromDataRow, "startIndex", (int)startIndex, "endIndex", (int)endIndex);
        }

        public Ranking Deserialize_ByLogin(string login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            const string selectStatement = "Select P.Nickname, P.Login, R.* FROM Rank R INNER JOIN Player P  on P.ID = R.PlayerID WHERE P.Login = @login";
            return SqlHelper.ExecuteClassQuery<Ranking>(selectStatement, RankingFromDataRow, "login", login);
        }

        public Ranking Deserialize_ByRank(int rank)
        {
            const string selectStatement = "Select P.Nickname, P.Login, R.* FROM Rank R INNER JOIN Player P  on P.ID = R.PlayerID WHERE R.[Rank] = @rank";
            return SqlHelper.ExecuteClassQuery<Ranking>(selectStatement, RankingFromDataRow, "rank", rank);
        }

        public uint GetTopRankingsCount()
        {
            const string selectStatement = "Select Count(PlayerID) From (Select Distinct PlayerID from ranking where [Rank] <= 3) P";
            return Convert.ToUInt32(SqlHelper.ExecuteScalar<int>(selectStatement));
        }

        public uint GetRanksCount()
        {
            const string selectStatement = "Select Count(*) From Rank";
            return Convert.ToUInt32(SqlHelper.ExecuteScalar<int>(selectStatement));
        }

        #endregion

        #region Non Public Methods

        private int? GetNextRankNumber(string login)
        {
            const string selectStatement = "Select R.Rank - 1 FROM Rank R INNER JOIN Player P on P.ID = R.PlayerID WHERE P.Login = @login";
            return SqlHelper.ExecuteScalar<int?>(selectStatement, "login", login);
        }

        private int? GetMaxRankNumber()
        {
            const string selectStatement = "Select Max(Rank) FROM Rank";
            return SqlHelper.ExecuteScalar<int?>(selectStatement);
        }

        private void ReCreateRankTable()
        {
            const string countStatement = "Select Count(*) From Challenge";
            int challengesCount = SqlHelper.ExecuteScalar<int>(countStatement);

            const string deleteStatement = "DELETE FROM Rank";
            SqlHelper.ExecuteNonQuery(deleteStatement);

            const string insertStatement = "INSERT INTO [Rank] Select PlayerID, ROWID as [Rank], AVG(Rank) as AverageRank, Count([Rank]) as RecordsCount, @challengesCount as ChallengesCount, (AVG(Rank) + (@challengesCount+1) / (Count([Rank])+1) * (@challengesCount - Count([Rank]))) as Score " +
                                           "From Ranking GROUP By  PlayerID order by (AVG(Rank) + (@challengesCount+1) / (Count([Rank])+1) * (@challengesCount - Count([Rank]))) ASC";

            SqlHelper.ExecuteNonQuery(insertStatement, "challengesCount", challengesCount);
        }

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
