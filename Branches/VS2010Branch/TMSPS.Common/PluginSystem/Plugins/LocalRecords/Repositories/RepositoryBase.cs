using System;
using NHibernate;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories
{
    public abstract class RepositoryBase : IRepositoryBase
    {
        protected PlayerCache PlayerCache { get { return PlayerCache.Instance; } }
        private ISession Session { get; set; }

        protected RepositoryBase() : this(null)
        {
            
        }

        protected RepositoryBase(ISession session)
        {
            Session = session;
        }

        protected void UseSession(Action<ISession> action)
        {
            if (Session == null)
            {
                using (ISession session = RepositoryFactory.OpenSession())
                {
                    action(session);
                }
            }
            else
                action(Session);
        }

        protected void UseStatelessSession(Action<IStatelessSession> action)
        {
            using (IStatelessSession session = RepositoryFactory.OpenStatelessSession())
            {
                action(session);
            }
        }
    }
}
