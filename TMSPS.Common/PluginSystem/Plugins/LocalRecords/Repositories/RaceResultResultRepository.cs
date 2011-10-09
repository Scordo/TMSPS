using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories
{
    public class RaceResultRepository : RepositoryBase, IRaceResultRepository
    {
        #region Properties

        private IRaceResultRepository Interface
        {
            get { return this; }
        }

        #endregion

        #region Constructors

        public RaceResultRepository()
        {

        }

        public RaceResultRepository(ISession session) : base(session)
        {

        }

        #endregion

        #region IRaceResultRepository

        void IRaceResultRepository.AddResult(RaceResultEntity raceResult)
        {
            UseSession(session =>
            {
                session.Save(raceResult);
                session.Flush();                   
            });
        }

        List<PositionStats> IRaceResultRepository.DeserializeListByMost(uint top, uint positionLimit)
        {
            List<PositionStats> positionStats = null;
            const string hqlQuery = "select PlayerId, count(*)" +
                                    "from RaceResultEntity " +
                                    "where Position <= :PositionLimit " +
                                    "group by PlayerId " +
                                    "order by count(*) desc";

            UseSession(session =>
            {
                List<object[]> list = session.CreateQuery(hqlQuery).SetInt32("PositionLimit", (int)positionLimit).SetMaxResults((int)top).Enumerable<object[]>().ToList();
                positionStats = list.ConvertAll(r => new PositionStats { PlayerId = Convert.ToInt32(r[0]), Amount = Convert.ToUInt32(r[1]) });
            });

            positionStats.ForEach(r => r.Nickname = PlayerCache.Instance.Get(r.PlayerId).Nickname);

            return positionStats;
        }

        #endregion

    }
}