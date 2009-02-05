using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL
{
    public class SessionAdapter : SQLBaseAdapter, ISessionAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionAdapter"/> class.
        /// </summary>
        public SessionAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        public SessionAdapter(ConnectionManager connectionManager): base(connectionManager)
        {
        }

        #endregion
    }
}