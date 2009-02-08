using System;
using System.Collections.Generic;
using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL
{
    public class PositionAdapter : SQLBaseAdapter, IPositionAdapter
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
        public PositionAdapter(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        #endregion

        #region Public Methods

        public void AddPosition(string login, string uniqueChallengeID, ushort position, ushort maxPosition)
        {
            if (login.IsNullOrTimmedEmpty())
                throw new ArgumentException("Login is null or empty.");

            if (uniqueChallengeID.IsNullOrTimmedEmpty())
                throw new ArgumentException("UniqueChallengeID is null or empty.");

            if (position == 0 || position > maxPosition)
                throw new ArgumentOutOfRangeException("position");

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Login", login.Trim()},
                {"UniqueChallengeID", uniqueChallengeID.Trim()},
                {"Position", Convert.ToInt16(position)},
                {"MaxPosition", Convert.ToInt16(maxPosition)}
            };

            SqlHelper.ExecuteNonQuery("Position_Add", parameters);
        }

        #endregion
    }
}
