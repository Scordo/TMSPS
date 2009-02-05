using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL
{
    public class RankAdapter : SQLBaseAdapter, IRankAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RankAdapter"/> class.
        /// </summary>
        public RankAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RankAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        public RankAdapter(ConnectionManager connectionManager): base(connectionManager)
        {
        }

        #endregion
    }
}