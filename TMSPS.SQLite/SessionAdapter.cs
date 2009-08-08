using System;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.SQLite
{
    public class SessionAdapter : BaseAdapter, ISessionAdapter
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
        public SessionAdapter(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        #endregion

        #region ISessionAdapter Members

        public void AddSession(string login, int challengeID, uint timeOrScore)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
