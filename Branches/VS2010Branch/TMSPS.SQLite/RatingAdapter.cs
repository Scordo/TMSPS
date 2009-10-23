using System;
using TMSPS.Core.Common;
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

        public Pair<double?, int> Vote(string login, int challengeID, ushort rating)
        {
            int? playerID = GetPlayerID(login);

            if (playerID == null)
                return null;

            const string userCountStatement = "Select Count(*) FROM [Rating] WHERE [PlayerID] = @PlayerID and [ChallengeID] = @ChallengeID";
            double? usersVoteCount = SqlHelper.ExecuteScalar<double?>(userCountStatement, "ChallengeID", challengeID, "PlayerID", playerID.Value);
            
            if (usersVoteCount == 0)
            {
                const string insertStatement = "INSERT INTO [Rating] ([PlayerID], [ChallengeID], [Value], [Created]) VALUES (@PlayerID, @ChallengeID, @Rating, @Created)";
                SqlHelper.ExecuteNonQuery(insertStatement, "PlayerID", playerID.Value, "ChallengeID", challengeID, "Rating", (int) rating, "Created", DateTime.Now);
            }
            else
            {
                const string updateStatement = "UPDATE [Rating] SET [LastChanged] = CURRENT_TIMESTAMP, [Value] = @Rating WHERE [PlayerID] = @PlayerID AND [ChallengeID] = @ChallengeID";
                SqlHelper.ExecuteNonQuery(updateStatement, "PlayerID", playerID.Value, "ChallengeID", challengeID, "Rating", (int)rating);    
            }

            return GetAverageVote(challengeID);
        }

        public double? GetVoteByLogin(string login, int challengeID)
        {
            int? playerID = GetPlayerID(login);

            if (playerID == null)
                return null;

            const string selectStatement = "Select [Value] FROM [Rating] WHERE [PlayerID] = @PlayerID and [ChallengeID] = @ChallengeID";
            return SqlHelper.ExecuteScalar<double?>(selectStatement, "PlayerID", playerID.Value, "ChallengeID", challengeID);
        }

        public Pair<double?, int> GetAverageVote(int challengeID)
        {
            const string selectStatement = "Select AVG([Value]) FROM [Rating] WHERE [ChallengeID] = @ChallengeID";
            const string countStatement = "Select Count([Value]) FROM [Rating] WHERE [ChallengeID] = @ChallengeID";

            double? averageVote = SqlHelper.ExecuteScalar<double?>(selectStatement, "ChallengeID", challengeID);
            int votesCount = SqlHelper.ExecuteScalar<int>(countStatement, "ChallengeID", challengeID);

            return new Pair<double?, int>{Value1 = averageVote, Value2 = votesCount};
        }

        #endregion
    }
}
