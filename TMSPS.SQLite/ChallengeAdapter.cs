using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public Challenge Deserialize(int challengeID)
        {
            throw new NotImplementedException();
        }

        public Challenge Deserialize(string uniqueID)
        {
            throw new NotImplementedException();
        }

        public int? IncreaseRaces(int challengeID)
        {
            throw new NotImplementedException();
        }

        public int? IncreaseRaces(string uniqueID)
        {
            throw new NotImplementedException();
        }

        public void IncreaseRaces(Challenge challenge)
        {
            throw new NotImplementedException();
        }

        public int DeleteTracksNotInProvidedList(List<string> uniqueTrackIDs)
        {
            throw new NotImplementedException();
        }

        public void DeleteTrack(string uniqueChallengeID)
        {
            throw new NotImplementedException();
        }

        public List<string> GetDrivenUniqueTrackIDs(string login)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
