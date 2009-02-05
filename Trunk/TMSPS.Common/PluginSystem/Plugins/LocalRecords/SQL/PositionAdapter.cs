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
    }
}
