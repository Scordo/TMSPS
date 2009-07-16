using System;
using System.Collections.Generic;
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

        #region Public Methods

        public double? Vote(string login, int challengeID, ushort rating)
        {
            if (rating > 8)
                rating = 8;

             if (login.IsNullOrTimmedEmpty())
                throw new ArgumentException("Login is null or empty.");

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Login", login.Trim()},
                {"ChallengeID", challengeID},
                {"Rating", Convert.ToInt16(rating)}
            };

            return SqlHelper.ExecuteScalar<double?>("Rating_Vote", parameters);
        }

        public double? GetVoteByLogin(string login, int challengeID)
        {
            if (login.IsNullOrTimmedEmpty())
                throw new ArgumentException("Login is null or empty.");

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Login", login.Trim()},
                {"ChallengeID", challengeID}
            };

            return SqlHelper.ExecuteScalar<double?>("Rating_GetVoteByLogin", parameters);
        }

        public double? GetAverageVote(int challengeID)
        {
            return SqlHelper.ExecuteScalar<double?>("Rating_GetAverageVote", "ChallengeID", challengeID);
        }


        #endregion

    }
}