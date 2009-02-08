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

        #endregion

    }
}