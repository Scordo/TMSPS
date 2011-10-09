using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories
{
    public class ChallengeRepository : RepositoryBase, IChallengeRepository
    {
        #region Properties

        private IChallengeRepository Interface
        {
            get { return this; }
        }

        #endregion

        #region Constructors

        public ChallengeRepository()
        {

        }

        public ChallengeRepository(ISession session) : base(session)
        {

        }

        #endregion

        #region IChallengeRepository

        ChallengeEntity IChallengeRepository.Get(string uid)
        {
            ChallengeEntity result = null;

            UseSession(session =>
            {
                result = session.Query<ChallengeEntity>().Where(c => c.UniqueId.Equals(uid)).FirstOrDefault();
            });

            if (result != null)
                ChallengeCache.Add(result, true);

            return result;
        }

        ChallengeEntity IChallengeRepository.IncreaseRaces(ChallengeEntity challenge)
        {
            if (challenge == null)
                throw new ArgumentNullException("challenge");

            ChallengeEntity loadedChallenge = Interface.Get(challenge.UniqueId);

            UseSession(session =>
            {
                if (loadedChallenge == null)
                {
                    challenge.Races++;
                    session.Save(challenge);
                }
                else
                {
                    loadedChallenge.Races++;
                    loadedChallenge.LastChanged = DateTime.Now;
                    session.Update(loadedChallenge);
                }

                session.Flush();
            });

            ChallengeEntity result = loadedChallenge ?? challenge;
            ChallengeCache.Add(result, true);

            return result;
        }

        int IChallengeRepository.DeleteTracksNotInProvidedList(List<string> uniqueTrackIDs)
        {
            // do not delete all maps
            if (uniqueTrackIDs == null || uniqueTrackIDs.Count == 0)
                return 0;

            HashSet<string> uniqueTrackIDsFromDB = null;
            
            UseSession(session =>
            {
                uniqueTrackIDsFromDB = GetUniqueChallengeIds(session);
                uniqueTrackIDsFromDB.ExceptWith(uniqueTrackIDs);

                foreach (string uniqueTrackID in uniqueTrackIDsFromDB)
                {
                    DeleteChallenge(session, ChallengeCache.Get(uniqueTrackID).Id.Value);
                }                   
            });
            

            return uniqueTrackIDsFromDB.Count;
        }



        List<string> IChallengeRepository.GetDrivenUniqueTrackIDs(int playerId)
        {
            List<string> result = null;
            UseSession(session =>
            {
                result = session.CreateQuery("Select c.UniqueId FROM RecordEntity as r INNER JOIN r.Challenge as c WHERE r.PlayerId = :PlayerId")
                                .SetInt32("PlayerId", playerId)
                                .Enumerable<string>()
                                .ToList();
            });

            return result;
        }

        int IChallengeRepository.Count()
        {
            int result = 0;

            UseSession(session => result = session.Query<ChallengeEntity>().Count());

            return result;
        }

        #endregion

        #region Non PUblic Methods

        private static void DeleteChallenge(ISession session,  int challengeId)
        {
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.CreateQuery("DELETE FROM LapResultEntity WHERE ChallengeId = :ChallengeId").SetInt32("ChallengeId", challengeId).ExecuteUpdate();
                session.CreateQuery("DELETE FROM RecordEntity WHERE ChallengeId = :ChallengeId").SetInt32("ChallengeId", challengeId).ExecuteUpdate();
                session.CreateQuery("DELETE FROM RatingEntity WHERE ChallengeId = :ChallengeId").SetInt32("ChallengeId", challengeId).ExecuteUpdate();
                session.CreateQuery("DELETE FROM ChallengeRankEntity WHERE ChallengeId = :ChallengeId").SetInt32("ChallengeId", challengeId).ExecuteUpdate();
                session.CreateQuery("DELETE FROM RaceResultEntity WHERE ChallengeId = :ChallengeId").SetInt32("ChallengeId", challengeId).ExecuteUpdate();
                session.CreateQuery("DELETE FROM ChallengeEntity WHERE Id = :ChallengeId").SetInt32("ChallengeId", challengeId).ExecuteUpdate();
                transaction.Commit();
            }
        }

        private static HashSet<string> GetUniqueChallengeIds(ISession session)
        {
            return new HashSet<string>(session.Query<ChallengeEntity>().Select(c => c.UniqueId), StringComparer.Ordinal);
        }

        #endregion

    }
}