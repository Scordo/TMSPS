using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL
{
    public class PlayerAdapter : SQLBaseAdapter, IPlayerAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerAdapter"/> class.
        /// </summary>
        public PlayerAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        public PlayerAdapter(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        #endregion
    }
}
