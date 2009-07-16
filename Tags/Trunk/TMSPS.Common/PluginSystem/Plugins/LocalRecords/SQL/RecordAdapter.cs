using System;
using System.Collections.Generic;
using System.Data;
using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL
{
    public class RecordAdapter : SQLBaseAdapter, IRecordAdapter
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
        public RecordAdapter(ConnectionManager connectionManager): base(connectionManager)
        {
        }

        #endregion

        #region Public Methods

        public void CheckAndWriteNewRecord(string login, int challengeID, int timeOrScore, out uint? oldLocalRecordPosition, out uint? newLocalRecordPosition, out bool newBest)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Login", login.Trim()},
                {"ChallengeID", challengeID},
                {"TimeOrScore", timeOrScore}
            };

            DataRow row = SqlHelper.ExecuteDataTable("Record_TryInsertOrUpdate", parameters).Rows[0];
            oldLocalRecordPosition = row["OldPosition"] == DBNull.Value ? null : (uint?)Convert.ToUInt32(row["OldPosition"]);
            newLocalRecordPosition = row["NewPosition"] == DBNull.Value ? null : (uint?)Convert.ToUInt32(row["NewPosition"]);
            newBest = Convert.ToInt16(row["NewBest"]) == 1;
        }

        public RankEntry GetFirstRecordForChallenge(int challengeID)
        {
            List<RankEntry> list = GetTopRecordsForChallenge(challengeID, 1);

            return list.Count == 0 ? null : list[0];
        }


        public List<RankEntry> GetTopRecordsForChallenge(int challengeID, uint maxRecords)
        {
            return SqlHelper.ExecuteClassListQuery<RankEntry>("Record_GetTopRecords", RankEntryFromDataRow, "ChallengeID", challengeID, "MaxRecords", Convert.ToInt32(maxRecords));
        }

        public uint? GetBestTime(string login, int challengeID)
        {
            int? bestTime = SqlHelper.ExecuteScalar<int?>("Record_GetBest", "Login", login, "ChallengeID", challengeID);

            return bestTime.HasValue ? (uint?) Convert.ToUInt32(bestTime) : null;
        }

        #endregion

        #region Non Public Methods

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