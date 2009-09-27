﻿using System;
using System.Collections.Generic;
using System.Data;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.SQLite
{
    public class ChallengeAdapter : BaseAdapter, IChallengeAdapter
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
        public ChallengeAdapter(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        #endregion

        #region IChallengeAdapter Members

        public void TryCreate(Challenge challenge)
        {
            Challenge loadedChallenge = Deserialize(challenge.UniqueID);

            if (loadedChallenge == null)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"UniqueID", challenge.UniqueID.Trim()},
                    {"Author", challenge.Author.Trim()},
                    {"Environment", challenge.Environment.Trim()},
                    {"Name", challenge.Name.Trim()},
                    {"Races", 0},
                    {"Created", DateTime.Now}
                };

                const string insertStatement = "INSERT INTO [Challenge] ([UniqueID], [Name], [Author], [Environment], [Races], [Created]) VALUES (@UniqueID, @Name, @Author, @Environment, @Races, @Created)";
                SqlHelper.ExecuteNonQuery(insertStatement, parameters);
                loadedChallenge = Deserialize(challenge.UniqueID);

                if (loadedChallenge == null)
                    throw new InvalidOperationException("Could not deserialize recently created challenge");
            }

            challenge.Assign(loadedChallenge);
        }

        public Challenge Deserialize(int challengeID)
        {
            const string selectStatement = "Select * From [Challenge] where [ID] = @ID";
            return SqlHelper.ExecuteClassQuery<Challenge>(selectStatement, ChallengeFromDataRow, "ID", challengeID);
        }

        public Challenge Deserialize(string uniqueID)
        {
            const string selectStatement = "Select * From [Challenge] where [UniqueID] = @UniqueID";
            return SqlHelper.ExecuteClassQuery<Challenge>(selectStatement, ChallengeFromDataRow, "UniqueID", uniqueID);
        }

        public int? IncreaseRaces(int challengeID)
        {
            const string updateStatement = "Update [Challenge] SET [Races] = [Races] + 1 WHERE ID = @ID";
            SqlHelper.ExecuteNonQuery(updateStatement, "ID", challengeID);

            const string selectStatement = "Select [Races] FROm [Challenge] WHERE [ID] = @ID";
            return SqlHelper.ExecuteScalar<int?>(selectStatement, "ID", challengeID);
        }

        public int? IncreaseRaces(string uniqueID)
        {
            const string updateStatement = "Update [Challenge] SET [Races] = [Races] + 1 WHERE UniqueID = @UniqueID";
            SqlHelper.ExecuteNonQuery(updateStatement, "UniqueID", uniqueID);

            const string selectStatement = "Select [Races] FROm [Challenge] WHERE [UniqueID] = @UniqueID";
            return SqlHelper.ExecuteScalar<int?>(selectStatement, "UniqueID", uniqueID);
        }

        public void IncreaseRaces(Challenge challenge)
        {
            TryCreate(challenge);
            IncreaseRaces(challenge.ID.Value);
            challenge.Races += 1;
        }

        public int DeleteTracksNotInProvidedList(List<string> uniqueTrackIDs)
        {
            // do not delete all maps
            if (uniqueTrackIDs == null || uniqueTrackIDs.Count == 0)
                return 0;

            const string selectStatement = "Select UniqueID From Challenge";
            HashSet<string> uniqueTrackIDsFromDB = new HashSet<string>(SqlHelper.ExecuteClassListQuery(selectStatement, (DataRow row) => Convert.ToString(row["UniqueID"])), StringComparer.Ordinal);
            uniqueTrackIDsFromDB.ExceptWith(uniqueTrackIDs);

            foreach (string uniqueTrackID in uniqueTrackIDsFromDB)
            {
                DeleteTrack(uniqueTrackID);
            }

            return uniqueTrackIDsFromDB.Count;
        }

        public void DeleteTrack(string uniqueChallengeID)
        {
            int? challengeID = GetChallengeID(uniqueChallengeID);

            if (!challengeID.HasValue)
                return;

            SqlHelper.ExecuteNonQuery("DELETE FROM [Session] WHERE ChallengeId = @challengeID", "@challengeID", challengeID.Value);
            SqlHelper.ExecuteNonQuery("DELETE FROM [Record] WHERE ChallengeId = @challengeID", "@challengeID", challengeID.Value);
            SqlHelper.ExecuteNonQuery("DELETE FROM [Rating] WHERE ChallengeId = @challengeID", "@challengeID", challengeID.Value);
            SqlHelper.ExecuteNonQuery("DELETE FROM [Ranking] WHERE ChallengeId = @challengeID", "@challengeID", challengeID.Value);
            SqlHelper.ExecuteNonQuery("DELETE FROM [Position] WHERE ChallengeId = @challengeID", "@challengeID", challengeID.Value);
            SqlHelper.ExecuteNonQuery("DELETE FROM [Challenge] WHERE ID = @challengeID", "@challengeID", challengeID.Value);
        }

        public List<string> GetDrivenUniqueTrackIDs(string login)
        {
            int? playerID = GetPlayerID(login);

            if (!playerID.HasValue)
                return new List<string>();

            const string selectStatement = "Select c.UniqueID FROM Record r INNER JOIN Challenge c on c.ID = r.ChallengeID WHERE r.PlayerID = @PlayerID";

            return SqlHelper.ExecuteClassListQuery(selectStatement, (DataRow r) => Convert.ToString(r["UniqueID"]), "PlayerID", playerID.Value);
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
            challengeSerializable.LastChanged = row["LastChanged"] == DBNull.Value ? null : (DateTime?) Convert.ToDateTime(row["LastChanged"]);
            challenge.UniqueID = Convert.ToString(row["UniqueID"]);
            challenge.Author = Convert.ToString(row["Author"]);
            challenge.Environment = Convert.ToString(row["Environment"]);
            challenge.Races = Convert.ToInt32(row["Races"]);

            return challenge;
        }

        #endregion
    }
}