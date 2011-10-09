using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories
{
    public class RecordRepository : RepositoryBase, IRecordRepository
    {
        #region Properties

        private IRecordRepository Interface
        {
            get { return this; }
        }

        #endregion

        #region Constructors

        public RecordRepository()
        {

        }

        public RecordRepository(ISession session) : base(session)
        {

        }

        #endregion

        #region IRecordRepository

        RecordState IRecordRepository.CheckAndWriteNewRecord(string login, int challengeID, int timeOrScore)
        {
            PlayerEntity player = PlayerCache.Get(login);

            if (player == null)
                throw new ArgumentException(string.Format("Login '{0}' does not exists in db", login), "login");

            RecordState result = new RecordState {ChallengeId = challengeID, CurrentTimeOrScore = timeOrScore};
            
            UseSession(session => 
            {
                RecordEntity record = session.Query<RecordEntity>().Where(r => r.PlayerId == player.Id && r.ChallengeId == challengeID).FirstOrDefault();
                IChallengeRankRepository challengeRepository = RepositoryFactory.Get<IChallengeRankRepository>(session);
                
                if (record != null)
                {
                    result.PrevTimeOrScore = record.TimeOrScore;

                    if (timeOrScore >= record.TimeOrScore)
                        return;

                    result.PrevPosition = (uint?) challengeRepository.GetChallengeRank(challengeID, player.Id.Value);
                    record.Created = DateTime.Now;
                    record.TimeOrScore = timeOrScore;
                    session.Update(record);
                }
                else
                {
                    record = new RecordEntity{ChallengeId = challengeID, PlayerId = player.Id.Value, TimeOrScore = timeOrScore, Created = DateTime.Now};
                    session.Save(record);
                }

                session.Flush();

                result.CurrentPosition = (uint) challengeRepository.GetChallengeRank(challengeID, player.Id.Value);
            });

            return result;
        }

        List<RankEntry> IRecordRepository.GetTopRecordsForChallenge(int challengeID, uint maxRecords)
        {
            List<RecordEntity> records = null;

            UseSession(session =>
            {
                records = session.Query<RecordEntity>().Where(r => r.ChallengeId == challengeID).OrderBy(r => r.TimeOrScore).ThenBy(r => r.Created).Take((int)maxRecords).ToList();
            });

            List<RankEntry> result = new List<RankEntry>();

            for (int i = 0; i < records.Count; i++)
            {
                RecordEntity record = records[i];
                result.Add(new RankEntry((ushort)(i + 1), record.Player.Login, record.Player.Nickname, (uint)record.TimeOrScore));
            }

            return result;
        }

        uint? IRecordRepository.GetBestTime(string login, int challengeID)
        {
            PlayerEntity player = PlayerCache.Get(login);

            if (player == null)
                return null;

            int timeOrScore = 0;

            UseSession(session =>
            {
                timeOrScore = session.Query<RecordEntity>().Where(r => r.PlayerId == player.Id && r.ChallengeId == challengeID).Select(r => r.TimeOrScore).FirstOrDefault();
            });

            return timeOrScore == 0 ? null : (uint?)timeOrScore;
        }

        #endregion

    }
}