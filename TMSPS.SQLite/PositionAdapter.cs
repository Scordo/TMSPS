using System;
using System.Collections.Generic;
using System.Data;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.SQLite
{
    public class PositionAdapter : BaseAdapter, IPositionAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionAdapter"/> class.
        /// </summary>
        public PositionAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        public PositionAdapter(ConnectionManager connectionManager)
            : base(connectionManager)
        {
        }

        #endregion

        #region IPositionAdapter Members

        public void AddPosition(string login, string uniqueChallengeID, ushort position, ushort maxPosition)
        {
            int? playerID = GetPlayerID(login);

            if (!playerID.HasValue)
                return;

            int? challengeID = GetChallengeID(uniqueChallengeID);

            if (!challengeID.HasValue)
                return;

            const string insertStatement = "INSERT INTO Position (PlayerID, ChallengeID, OwnPosition, MaxPosition, Created) VALUES (@PlayerID, @ChallengeID, @Position, @MaxPosition, @Created)";
            SqlHelper.ExecuteNonQuery(insertStatement, "PlayerID", playerID.Value, "ChallengeID", challengeID.Value, "Position", (int)position, "MaxPosition", (int)maxPosition, "Created", DateTime.Now);
        }

        public List<PositionStats> DeserializeListByMost(uint top, uint positionLimit)
        {
            const string selectStatement = "Select P.Nickname, M.*" +
                                           "FROM (Select PlayerID, Count(OwnPosition) as PositionsCount FROM Position WHERE OwnPosition <= @positionLimit Group by  PlayerID Order by PositionsCount desc) M " +
                                           "INNER JOIN Player P  on P.Id = M.PlayerID LIMIT @top";

            return SqlHelper.ExecuteClassListQuery<PositionStats>(selectStatement, PositionStatsFromDataRow, "positionLimit", (int)positionLimit, "top", (int)top);
        }

        #endregion

        #region Non Public Methods

        private static PositionStats PositionStatsFromDataRow(DataRow row)
        {
            string nickname = Convert.ToString(row["Nickname"]);
            int playerID = Convert.ToInt32(row["PlayerID"]);
            uint positionsCount = Convert.ToUInt32(row["PositionsCount"]);

            return new PositionStats(playerID, nickname, positionsCount);
        }

        #endregion
    }
}
