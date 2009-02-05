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
    }
}