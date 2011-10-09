using System;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories
{
    public class RepositoryFactory
    {
        private static ISessionFactory SessionFactory { get; set; }

        public static void Init(DatabaseType databaseType, string connectionString)
        {
            if (SessionFactory != null)
                return;

            IPersistenceConfigurer databaseConfigurer;

            switch (databaseType)
            {
                case DatabaseType.MsSql2008:
                    databaseConfigurer = MsSqlConfiguration.MsSql2008.ConnectionString(connectionString).AdoNetBatchSize(100);
                    break;
                case DatabaseType.MySql:
                    databaseConfigurer = MySQLConfiguration.Standard.ConnectionString(connectionString).AdoNetBatchSize(100);
                    break;
                default:
                    throw new InvalidOperationException(String.Format("DatabaseType {0} was forgotten to be implemented!", databaseType));
            }

            SessionFactory = Fluently.Configure()
                .Database(databaseConfigurer)
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<RepositoryFactory>())
                .BuildSessionFactory();
        }

        public static ISession OpenSession()
        {
            if (SessionFactory == null)
                throw new InvalidOperationException("Please call Init(..) once before opening a session!");

            return SessionFactory.OpenSession();
        }

        public static IStatelessSession OpenStatelessSession()
        {
            if (SessionFactory == null)
                throw new InvalidOperationException("Please call Init(..) once before opening a session!");

            return SessionFactory.OpenStatelessSession();
        }

        public static TRepository Get<TRepository>(ISession session = null) where TRepository : IRepositoryBase
        {
            Type repositoryType = typeof (TRepository);
            if (repositoryType.IsAssignableFrom(typeof(IChallengeRankRepository)))
                return (TRepository)(IChallengeRankRepository)new ChallengeRankRepository(session);

            if (repositoryType.IsAssignableFrom(typeof(IChallengeRepository)))
                return (TRepository)(IRepositoryBase)new ChallengeRepository(session);

            if (repositoryType.IsAssignableFrom(typeof(ILaptResultRepository)))
                return (TRepository)(IRepositoryBase)new LapResultRepository(session);

            if (repositoryType.IsAssignableFrom(typeof(IPlayerRepository)))
                return (TRepository)(IRepositoryBase)new PlayerRepository(session);

            if (repositoryType.IsAssignableFrom(typeof(IRaceResultRepository)))
                return (TRepository)(IRaceResultRepository)new RaceResultRepository(session);

            if (repositoryType.IsAssignableFrom(typeof(IRatingRepository)))
                return (TRepository)(IRatingRepository)new RatingRepository(session);

            if (repositoryType.IsAssignableFrom(typeof(IRecordRepository)))
                return (TRepository)(IRepositoryBase)new RecordRepository(session);

            if (repositoryType.IsAssignableFrom(typeof(IServerRankRepository)))
                return (TRepository)(IRepositoryBase)new ServerRankRepository(session);
            

            throw new ArgumentException(String.Format("The repository type '{0}' is not supported!", repositoryType.FullName));
        }
    }

    public enum DatabaseType { MsSql2008, MySql }
}
