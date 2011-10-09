using System;
using NHibernate;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories
{
    public class LapResultRepository : RepositoryBase, ILaptResultRepository
    {
        #region Constructors

        public LapResultRepository()
        {

        }

        public LapResultRepository(ISession session) : base(session)
        {

        }

        #endregion

        #region ILaptResultRepository

        void ILaptResultRepository.AddLapResult(LapResultEntity lapResult)
        {
            if (lapResult == null)
                throw new ArgumentNullException("lapResult");

            UseSession(session =>
            {
                session.Save(lapResult);
                session.Flush();
            });
        }

        #endregion
    }
}