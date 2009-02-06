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

        public bool Create(Challenge challenge)
        {
            if (challenge == null)
                throw new ArgumentNullException("challenge");

            if (challenge.ID.HasValue)
                throw new ArgumentException("Challenge is already present in database!");

            if (challenge.UniqueID.IsNullOrTimmedEmpty())
                throw new ArgumentException("UniqueID is null or empty.");

            if (challenge.Author.IsNullOrTimmedEmpty())
                throw new ArgumentException("Author is null or empty.");

            if (challenge.Environment.IsNullOrTimmedEmpty())
                throw new ArgumentException("Environment is null or empty.");

            if (challenge.Name.IsNullOrTimmedEmpty())
                throw new ArgumentException("Name is null or empty.");

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
                challengeSerializable.LastChanged = Convert.ToDateTime(row["Created"]);
            }

            return false;
        }

        public Challenge Deserialize(int challengeID)
        {
            return SqlHelper.ExecuteClassQuery<Challenge>("Challenge_Deserialize_ByID", ChallengeFromDataRow, "ID", challengeID);
        }

        public Challenge Deserialize(string uniqueID)
        {
            return SqlHelper.ExecuteClassQuery<Challenge>("Challenge_Deserialize_ByUniqueID", ChallengeFromDataRow, "UniqueID", uniqueID);
        }

        public int IncreaseRaces(int challengeID)
        {
            return SqlHelper.ExecuteScalar<int>("Challenge_IncreaseRaces");
        }

        #endregion

        #region Non Public Methods

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