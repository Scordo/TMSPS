using System.Collections.Generic;
using NHibernate;
using NHibernate.Linq;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;
using System.Linq;
using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories
{
    public class ServerRankRepository: RepositoryBase, IServerRankRepository
    {
        #region Properties

        private IServerRankRepository Interface
        {
            get { return this; }
        }

        #endregion

        #region Constructors

        public ServerRankRepository()
        {

        }

        public ServerRankRepository(ISession session) : base(session)
        {

        }

        #endregion

        #region IServerRankRepository

        uint IServerRankRepository.GetRanksCount()
        {
            uint result = 0;

            UseSession(session => result = (uint) session.Query<ServerRankEntity>().Count());

            return result;
        }

        Ranking IServerRankRepository.Get(int playerId)
        {
            ServerRankEntity result = null;

            UseSession(session => result = session.Query<ServerRankEntity>().FirstOrDefault(r => r.PlayerId == playerId));

            if (result == null)
                return null;

            return ServerRankEntityToRanking(result);
        }

        Ranking IServerRankRepository.GetNextRank(int playerId)
        {
            Ranking ownRanking = Interface.Get(playerId);
            Ranking result = null;

            UseSession(session =>
            {
                int rankToGet;

                if (ownRanking != null)
                {
                    if (ownRanking.CurrentRank == 1)
                        return;

                    rankToGet = (int) ownRanking.CurrentRank - 1;
                }
                else
                    rankToGet = session.Query<ServerRankEntity>().Count();

                result = GetByRank(rankToGet, session);
            });
            
            return result;
        }

        List<Ranking> IServerRankRepository.GetList(uint top)
        {
            return Interface.GetList(0, top);
        }

        List<Ranking> IServerRankRepository.GetList(uint startIndex, uint endIndex)
        {
            List<ServerRankEntity> ranks = null;
            UseSession(session => ranks = session.Query<ServerRankEntity>().OrderBy(sr => sr.Rank).Skip((int)startIndex).Take((int)(endIndex - startIndex)).ToList());

            return ranks.ConvertAll(ServerRankEntityToRanking);
        }

        void IServerRankRepository.ReCreateRanks()
        {
            const int BATCH_SIZE = 50;
            int challengesCount = RepositoryFactory.Get<IChallengeRepository>().Count();

            UseSession(session =>
            {
                session.CreateQuery("DELETE FROM ServerRankEntity").ExecuteUpdate();

                const string hqlQuery = "select " +
                                        "   PlayerId," +
                                        "   avg(Rank)," +
                                        "   count(Rank) " +
                                        "from " +
                                        "   ChallengeRankEntity " +
                                        "group by " +
                                        "   PlayerId";

                List<ServerRankEntity> rankings = session.CreateQuery(hqlQuery).Enumerable<object[]>().Select(oa => ServerRankingEntityFromObjectArray(oa, challengesCount)).OrderBy(r => r.Score).ToList();
                rankings.ForEach((r, i) => { r.Rank = i + 1; r.ChallengesCount = challengesCount; });

                session.SetBatchSize(BATCH_SIZE);

                rankings.ForEach((rank, index) =>
                {
                    session.Save(rank);

                    if (index + 1 % BATCH_SIZE == 0)
                    {
                        session.Flush();
                        session.Clear();
                    }
                });

                session.Flush();
            });
        }

        #endregion

        #region Non Public Methods

        private Ranking GetByRank(int rank, ISession session)
        {
            ServerRankEntity serverRank = session.Query<ServerRankEntity>().FirstOrDefault(r => r.Rank == rank);

            if (serverRank == null)
                return null;

            return ServerRankEntityToRanking(serverRank);
        }

        private Ranking ServerRankEntityToRanking(ServerRankEntity serverRank)
        {
            PlayerEntity player = PlayerCache.Get(serverRank.PlayerId);

            return new Ranking
            {
                AverageRank = (double)serverRank.AverageRank,
                ChallengesCount = serverRank.ChallengesCount,
                CurrentRank = (uint)serverRank.Rank,
                RecordsCount = serverRank.RecordsCount,
                Score = (double)serverRank.Score,
                PlayerID = serverRank.PlayerId,
                Login = player.Login,
                Nickname = player.Nickname
            };
        }

        private static ServerRankEntity ServerRankingEntityFromObjectArray(object[] objects, int challengesCount)
        {
            double averageRank = Convert.ToDouble(objects[1]);
            int recordsCount = Convert.ToInt32(objects[2]);

            return new ServerRankEntity
            {
                PlayerId = Convert.ToInt32(objects[0]),
                AverageRank = (int?)Math.Ceiling(averageRank),
                RecordsCount = recordsCount,
                Score = GetScore(averageRank, challengesCount, recordsCount)
            };
        }

        private static decimal GetScore(double averageRank, int challengesCount, int recordsCount)
        {
            return Convert.ToDecimal(averageRank + ((challengesCount + 1) / (recordsCount + 1) * (challengesCount - recordsCount)));
        }

        #endregion
    }
}