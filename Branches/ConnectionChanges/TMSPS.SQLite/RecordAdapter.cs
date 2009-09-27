using System;
using System.Collections.Generic;
using System.Data;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.SQLite
{
    public class RecordAdapter : BaseAdapter, IRecordAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordAdapter"/> class.
        /// </summary>
        public RecordAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        public RecordAdapter(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        #endregion

        #region IRecordAdapter Members

        public void CheckAndWriteNewRecord(string login, int challengeID, int timeOrScore, out uint? oldLocalRecordPosition, out uint? newLocalRecordPosition, out bool newBest)
        {
            newBest = false;
            oldLocalRecordPosition = null;
            newLocalRecordPosition = null;

            int? playerID = GetPlayerID(login);

            if (!playerID.HasValue)
                return;

            int? oldPosition, oldTimeOrScore;
            GetPositionAndTimeOrScore(playerID.Value, challengeID, out oldPosition, out oldTimeOrScore);

            if (oldPosition.HasValue)
            {
                const string updateStatement = "Update Record SET TimeOrScore = @TimeOrScore, LastChanged = CURRENT_TIMESTAMP WHERE PlayerID = @PlayerID AND ChallengeID = @ChallengeID AND TimeOrScore > @TimeOrScore";
                SqlHelper.ExecuteNonQuery(updateStatement, "PlayerID", playerID.Value, "ChallengeID", challengeID, "TimeOrScore", timeOrScore);
            }
            else
            {
                const string insertStatement = "INSERT INTO Record (PlayerID, ChallengeID, TimeOrScore, Created, LastChanged) VALUES (@PlayerID, @ChallengeID, @TimeOrScore, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
                SqlHelper.ExecuteNonQuery(insertStatement, "PlayerID", playerID.Value, "ChallengeID", challengeID, "TimeOrScore", timeOrScore);
            }

            int? newPosition, newTimeOrScore;
            GetPositionAndTimeOrScore(playerID.Value, challengeID, out newPosition, out newTimeOrScore);

            newBest = !oldPosition.HasValue || (newTimeOrScore < oldTimeOrScore);
            oldLocalRecordPosition = oldPosition.HasValue ? (uint?) Convert.ToUInt32(oldPosition.Value) : null;
            newLocalRecordPosition = Convert.ToUInt32(newPosition.Value);
        }

        public List<RankEntry> GetTopRecordsForChallenge(int challengeID, uint maxRecords)
        {
            const string selectStatement = "Select 1 as [Rank], P.Login as Login, P.Nickname as Nickname, R.TimeOrScore as TimeOrScore " +
                                           "FROM Record R INNER JOIN Player P on P.ID = R.PlayerID " +
                                           "WHERE R.ChallengeID = @ChallengeID " +
                                           "ORDER BY R.TimeOrScore asc, R.LastChanged asc " +
                                           "LIMIT @MaxRecords";

            List<RankEntry> result = SqlHelper.ExecuteClassListQuery<RankEntry>(selectStatement, RankEntryFromDataRow, "ChallengeID", challengeID, "MaxRecords", Convert.ToInt32(maxRecords));

            for (ushort i = 0; i< result.Count; i++)
            {
                result[i].Rank += i;
            }

            return result;
        }

        public uint? GetBestTime(string login, int challengeID)
        {
            const string selectStatement = "Select [TimeOrScore] FROM [Record] R INNER JOIN Player P ON R.PlayerID = P.ID WHERE R.ChallengeID = @ChallengeID AND P.Login = @Login";
            int? bestTime = SqlHelper.ExecuteScalar<int?>(selectStatement, "Login", login, "ChallengeID", challengeID);

            return bestTime.HasValue ? (uint?)Convert.ToUInt32(bestTime) : null;
        }

        #endregion

        #region Non Public Methods

        private void GetPositionAndTimeOrScore(int playerID, int challengeID, out int? position, out int? timeOrScore)
        {
            position = null;

            const string selectTOSStatement = "Select TimeOrScore FROM Record WHERE PlayerID = @PlayerID";
            timeOrScore = SqlHelper.ExecuteScalar<int?>(selectTOSStatement, "PlayerID", playerID);

            if (!timeOrScore.HasValue)
                return;

            const string selectStatement = "Select PlayerID FROM Record WHERE ChallengeID = @ChallengeID order by  TimeOrScore asc,  LastChanged asc";
            List<int> orderedPlayers = SqlHelper.ExecuteClassListQuery(selectStatement, (DataRow r) => Convert.ToInt32(r["PlayerID"]), "PlayerID", playerID, "ChallengeID", challengeID);

            int playerIndex = orderedPlayers.IndexOf(playerID);

            if (playerIndex != -1)
                position = playerIndex + 1;
        }

        private static RankEntry RankEntryFromDataRow(DataRow row)
        {
            ushort rank = Convert.ToUInt16(row["Rank"]);
            string login = Convert.ToString(row["Login"]);
            string nickname = Convert.ToString(row["Nickname"]);
            uint timeOrScore = Convert.ToUInt32(row["TimeOrScore"]);

            return new RankEntry(rank, login, nickname, timeOrScore);
        }

        #endregion
    }
}
