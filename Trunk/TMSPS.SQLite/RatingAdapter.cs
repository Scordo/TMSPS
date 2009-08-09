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
            int? playerID = GetPlayerID(login);

            if (playerID == null)
                return null;

            const string countStatement = "Select Count(*) FROM [Rating] WHERE [PlayerID] = @PlayerID and [ChallengeID] = @ChallengeID";
            int count = SqlHelper.ExecuteScalar<int>(countStatement, "ChallengeID", challengeID);

            if (count == 0)
            {
                const string insertStatement = "INSERT INTO [Rating] ([PlayerID], [ChallengeID], [Value]) VALUES (@PlayerID, @ChallengeID, @Rating)";
                SqlHelper.ExecuteNonQuery(insertStatement, "PlayerID", playerID.Value, "ChallengeID", challengeID, "Rating", (int) rating);
                return rating;
            }

            const string updateStatement = "UPDATE [Rating] SET [LastChanged] = CURRENT_TIMESTAMP, [Value] = @Rating WHERE [PlayerID] = @PlayerID AND [ChallengeID] = @ChallengeID";
            SqlHelper.ExecuteNonQuery(updateStatement, "PlayerID", playerID.Value, "ChallengeID", challengeID, "Rating", (int)rating);

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

        public double? GetAverageVote(int challengeID)
        {
            const string selectStatement = "Select AVG([Value]) FROM [Rating] WHERE [ChallengeID] = @ChallengeID";

            return SqlHelper.ExecuteScalar<double?>(selectStatement, "ChallengeID", challengeID);
        }

        #endregion
    }
}
