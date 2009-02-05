using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL
{
    public class ChallengeAdapter : SQLBaseAdapter, IChallengeAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChallengeAdapter"/> class.
        /// </summary>
        public ChallengeAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChallengeAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        public ChallengeAdapter(ConnectionManager connectionManager): base(connectionManager)
        {
        }

        #endregion
    }
}