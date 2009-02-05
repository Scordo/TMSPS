using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL
{
    public class RatingAdapter : SQLBaseAdapter, IRatingAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RatingAdapter"/> class.
        /// </summary>
        public RatingAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RatingAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        public RatingAdapter(ConnectionManager connectionManager): base(connectionManager)
        {
        }

        #endregion
    }
}