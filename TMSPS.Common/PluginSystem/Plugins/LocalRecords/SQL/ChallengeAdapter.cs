using System;
using System.Collections.Generic;
using System.Data;
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

        #region Public Methods

    	public void TryCreate(Challenge challenge)
        {
            if (challenge == null)
                throw new ArgumentNullException("challenge");

            if (challenge.ID.HasValue)
                throw new ArgumentException("Challenge is already present in database!");

        	ValidateChallenge(challenge);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"UniqueID", challenge.UniqueID.Trim()},
                {"Author", challenge.Author.Trim()},
                {"Environment", challenge.Environment.Trim()},
                {"Name", challenge.Name.Trim()},
                {"Races", 0}
            };

            DataTable resultTable = SqlHelper.ExecuteDataTable("Challenge_Create", parameters);

            if (resultTable.Rows.Count > 0)
            {
                IChallengeSerializable challengeSerializable = challenge;
                DataRow row = resultTable.Rows[0];

                challengeSerializable.ID = Convert.ToInt32(row["ID"]);
                challengeSerializable.Created = Convert.ToDateTime(row["Created"]);
				challengeSerializable.LastChanged = row["LastChanged"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(row["LastChanged"]);
            }
        }

        public Challenge Deserialize(int challengeID)
        {
            return SqlHelper.ExecuteClassQuery<Challenge>("Challenge_Deserialize_ByID", ChallengeFromDataRow, "ID", challengeID);
        }

        public Challenge Deserialize(string uniqueID)
        {
            return SqlHelper.ExecuteClassQuery<Challenge>("Challenge_Deserialize_ByUniqueID", ChallengeFromDataRow, "UniqueID", uniqueID);
        }

    	/// <exception cref="ArgumentNullException"><c>challenge</c> is null.</exception>
    	public void IncreaseRaces(Challenge challenge)
		{
			if (challenge == null)
				throw new ArgumentNullException("challenge");

			ValidateChallenge(challenge);

			Dictionary<string, object> parameters = new Dictionary<string, object>
            {
				{"ID", challenge.ID},
                {"UniqueID", challenge.UniqueID.Trim()},
                {"Author", challenge.Author.Trim()},
                {"Environment", challenge.Environment.Trim()},
                {"Name", challenge.Name.Trim()},
            };

			DataTable resultTable = SqlHelper.ExecuteDataTable("Challenge_IncreaseRaces_ByChallenge", parameters);

			if (resultTable.Rows.Count > 0)
			{
				IChallengeSerializable challengeSerializable = challenge;
				DataRow row = resultTable.Rows[0];

				challengeSerializable.ID = Convert.ToInt32(row["ID"]);
				challengeSerializable.Created = Convert.ToDateTime(row["Created"]);
				challengeSerializable.LastChanged = row["LastChanged"] == DBNull.Value ? null : (DateTime?) Convert.ToDateTime(row["LastChanged"]);
				challenge.Races = Convert.ToInt32(row["Races"]);
			}
		}

        public int? IncreaseRaces(int challengeID)
        {
			return SqlHelper.ExecuteScalar<int?>("Challenge_IncreaseRaces_ByID", "ID", challengeID);
        }

		public int? IncreaseRaces(string uniqueID)
		{
			return SqlHelper.ExecuteScalar<int?>("Challenge_IncreaseRaces_ByUniqueID", "UniqueID", uniqueID);
		}

        public int DeleteTracksNotInProvidedList(List<string> uniqueTrackIDs)
        {
            // do not delete all maps
            if (uniqueTrackIDs == null || uniqueTrackIDs.Count == 0)
                return 0;

			HashSet<string> uniqueTrackIDsFromDB = new HashSet<string>(SqlHelper.ExecuteClassListQuery("Challenge_GetAllUniqueTrackIDs", (DataRow row) => Convert.ToString(row["UniqueID"])), StringComparer.Ordinal);
            uniqueTrackIDsFromDB.ExceptWith(uniqueTrackIDs);

        	foreach (string uniqueTrackID in uniqueTrackIDsFromDB)
        	{
        		DeleteTrack(uniqueTrackID);
        	}

            return uniqueTrackIDsFromDB.Count;
        }

		public void DeleteTrack(string uniqueChallengeID)
		{
			SqlHelper.ExecuteNonQuery("Challenge_Delete", "challengeUniqueID", uniqueChallengeID);
		}

        #endregion

        #region Non Public Methods

    	private static void ValidateChallenge(Challenge challenge)
		{
			if (challenge.UniqueID.IsNullOrTimmedEmpty())
				throw new ArgumentException("UniqueID is null or empty.");

			if (challenge.Author.IsNullOrTimmedEmpty())
				throw new ArgumentException("Author is null or empty.");

			if (challenge.Environment.IsNullOrTimmedEmpty())
				throw new ArgumentException("Environment is null or empty.");

			if (challenge.Name.IsNullOrTimmedEmpty())
				throw new ArgumentException("Name is null or empty.");
		}

        // <summary>
        /// Creates an instance of Challenge extracting the necessary data from the provided datarow.
        /// </summary>
        /// <param name="row">The data row.</param>
        /// <returns></returns>
        private static Challenge ChallengeFromDataRow(DataRow row)
        {
            Challenge challenge = new Challenge();
            IChallengeSerializable challengeSerializable = challenge;

            challengeSerializable.ID = Convert.ToInt32(row["ID"]);
            challengeSerializable.Created = Convert.ToDateTime(row["Created"]);
            challengeSerializable.LastChanged = Convert.ToDateTime(row["LastChanged"]);
            challenge.UniqueID = Convert.ToString(row["UniqueID"]);
            challenge.Author = Convert.ToString(row["Author"]);
            challenge.Environment = Convert.ToString(row["Environment"]);
            challenge.Races = Convert.ToInt32(row["Races"]);

            return challenge;
        }

        #endregion
    }
}