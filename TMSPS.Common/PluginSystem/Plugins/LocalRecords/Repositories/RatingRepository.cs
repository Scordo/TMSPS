using System;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories
{
    public class RatingRepository : RepositoryBase, IRatingRepository
    {
        #region Properties

        private IRatingRepository Interface
        {
            get { return this; }
        }

        #endregion

        #region Constructors

        public RatingRepository()
        {

        }

        public RatingRepository(ISession session) : base(session)
        {

        }

        #endregion

        #region IRatingRepository

        void IRatingRepository.Rate(int playerId, int challengeId, ushort value)
        {
            UseSession(session =>
            {
                RatingEntity ratingEntity = Interface.Get(playerId, challengeId);

                if (ratingEntity != null)
                {
                    if (ratingEntity.Value == value)
                        return;

                    ratingEntity.Value = (short)value;
                    ratingEntity.LastChanged = DateTime.Now;
                    session.Update(ratingEntity);
                }
                else
                {
                    ratingEntity = new RatingEntity { ChallengeId = challengeId, PlayerId = playerId, Value = (short)value };
                    session.Save(ratingEntity);
                }

                session.Flush();
            });
        }

        RatingEntity IRatingRepository.Get(int playerId, int challengeId)
        {
            RatingEntity result = null;

            UseSession(session =>
            {
                result = session.Query<RatingEntity>().Where(r => r.PlayerId == playerId && r.ChallengeId ==challengeId).FirstOrDefault();
            });

            return result;
        }

        Pair<double?, int> IRatingRepository.Average(int challengeID)
        {
            Pair<double?, int> result = null;
            
            UseSession(session =>
            {
                int count = session.Query<RatingEntity>().Count(r => r.ChallengeId == challengeID);
                double? average = count == 0 ? null : (double?) session.Query<RatingEntity>().Where(r => r.ChallengeId == challengeID).Average(r => r.Value);

                result = new Pair<double?, int> { Value1 = average, Value2 = count };
            });

            return result;
        }

        #endregion
    }
}