using System;
using System.Collections.Generic;
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

        #region Public Methods

        public void AddSession(string login, int challengeID, uint timeOrScore)
        {
            if (login.IsNullOrTimmedEmpty())
                throw new ArgumentException("Login is null or empty.");

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Login", login.Trim()},
                {"ChallengeID", challengeID},
                {"TimeOrScore", Convert.ToInt32(timeOrScore)}
            };

            SqlHelper.ExecuteNonQuery("Session_Add", parameters);
        }

        #endregion
    }
}