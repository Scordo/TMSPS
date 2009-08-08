using System;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.SQLite
{
    public class RatingAdapter : BaseAdapter, IRatingAdapter
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
        public RatingAdapter(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        #endregion

        #region IRatingAdapter Members

        public double? Vote(string login, int challengeID, ushort rating)
        {
            throw new NotImplementedException();
        }

        public double? GetVoteByLogin(string login, int challengeID)
        {
            throw new NotImplementedException();
        }

        public double? GetAverageVote(int challengeID)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
