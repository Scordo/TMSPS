using System;
using System.Collections.Generic;
using System.Data;
using TMSPS.Core.Common;
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

        public Pair<double?, int> Vote(string login, int challengeID, ushort rating)
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

            return SqlHelper.ExecuteClassQuery<Pair<double?, int>>("Rating_Vote", VoteInfoFromDataRow, parameters);
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

        public Pair<double?, int> GetAverageVote(int challengeID)
        {
            return SqlHelper.ExecuteClassQuery<Pair<double?, int>>("Rating_GetAverageVote", VoteInfoFromDataRow, "ChallengeID", challengeID);
        }

        #endregion

        private static Pair<double?, int> VoteInfoFromDataRow(DataRow row)
        {
            Pair<double?, int> result = new Pair<double?, int>();
            result.Value1 = row["AverageVote"] == DBNull.Value ? null : (double?)Convert.ToInt32(row["AverageVote"]);
            result.Value2 = Convert.ToInt32(row["VotesCount"]);

            return result;
        }
    }
}