using System;
using System.Collections.Generic;
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
        public PositionAdapter(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        #endregion

        #region IPositionAdapter Members

        public void AddPosition(string login, string uniqueChallengeID, ushort position, ushort maxPosition)
        {
            throw new NotImplementedException();
        }

        public List<PositionStats> DeserializeListByMost(uint top, uint positionLimit)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
