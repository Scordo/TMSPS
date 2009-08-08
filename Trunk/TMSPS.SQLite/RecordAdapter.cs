using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public List<RankEntry> GetTopRecordsForChallenge(int challengeID, uint maxRecords)
        {
            throw new NotImplementedException();
        }

        public RankEntry GetFirstRecordForChallenge(int challengeID)
        {
            throw new NotImplementedException();
        }

        public uint? GetBestTime(string login, int challengeID)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
