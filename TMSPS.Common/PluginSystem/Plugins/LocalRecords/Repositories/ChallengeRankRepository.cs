using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories
{
    public class ChallengeRankRepository : RepositoryBase, IChallengeRankRepository
    {
        #region Properties

        private IChallengeRankRepository Interface
        {
            get { return this; }
        }

        #endregion

        #region Constructors

        public ChallengeRankRepository()
        {

        }

        public ChallengeRankRepository(ISession session) : base(session)
        {

        }

        #endregion

        #region IChallengeRankRepository

        int? IChallengeRankRepository.GetChallengeRank(int challengeId, int playerId)
        {
            List<int> playerIds = null;
            
            UseSession(session =>
            {
                playerIds = new List<int>(session.Query<RecordEntity>().Where(r => r.ChallengeId == challengeId).OrderBy(r => r.TimeOrScore).Select(r => r.PlayerId));                   
            });
            
            int rank = playerIds.FindIndex(lpi => lpi == playerId);

            return rank == -1 ? null : (int?)rank + 1;
        }

        void IChallengeRankRepository.RecreateForChallenge(int challengeId)
        {
            UseSession(session => 
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    List<int> playerIds = new List<int>(session.Query<RecordEntity>().Where(r => r.ChallengeId == challengeId).OrderBy(r => r.TimeOrScore).Select(r => r.PlayerId));

                    session.CreateQuery("delete ChallengeRankEntity where ChallengeId = :ChallengeId").SetInt32("ChallengeId", challengeId).ExecuteUpdate();

                    for (int i = 0; i < playerIds.Count; i++)
                    {
                        session.Save(new ChallengeRankEntity { ChallengeId = challengeId, PlayerId = playerIds[i], Rank = i + 1 });
                    }

                    transaction.Commit();
                }
            });
        }

        uint IChallengeRankRepository.GetChallengeRankLeadersHavingTop3RanksCount()
        {
            uint result = 0;
            
            UseSession(session => 
            {
                result = (uint) session.Query<ChallengeRankEntity>().Where(cr => cr.Rank <= 3).GroupBy(cr => cr.PlayerId).Count();
            });

            return result;
        }

        PagedList<TopRankingEntry> IChallengeRankRepository.GetChallengeRankLeadersHavingTop3Ranks(uint startIndex, uint endIndex)
        {
            PagedList<TopRankingEntry> result = null; 

            UseSession(session =>
            {
                List<ChallengeRankEntity> ranks = session.Query<ChallengeRankEntity>().Where(cr => cr.Rank <= 3).ToList();

                if (ranks.Count == 0)
                {
                    result = new PagedList<TopRankingEntry> { VirtualCount = 0, StartIndex = (int)startIndex };
                    return;
                }

                IEnumerable<IGrouping<int, ChallengeRankEntity>> groupedRanks = ranks.GroupBy(r => r.PlayerId);

                List<TopRankingEntry> tempList = new List<TopRankingEntry>();

                groupedRanks.ForEach(ranksByPlayer =>
                {
                    uint one = 0, two = 0, three = 0;

                    ranksByPlayer.ForEach(cr =>
                    {
                        if (cr.Rank == 1)
                            one++;
                        if (cr.Rank == 2)
                            two++;
                        if (cr.Rank == 3)
                            three++;
                    });

                    tempList.Add(new TopRankingEntry{FirstRecords =  one, SecondRecords = two, ThirdRecords = three, Login = ranksByPlayer.Key.ToString()});
                });

                // set the virtual count to all players having a rank ranging from 1-3
                int virtualCount = tempList.Count;
                
                // order the list by positions
                tempList = tempList.OrderByDescending(r => r.FirstRecords)
                                   .ThenByDescending(r => r.SecondRecords)
                                   .ThenByDescending(r => r.ThirdRecords)
                                   .Skip((int)startIndex)
                                   .Take((int)(endIndex - startIndex))
                                   .Select((r, i) =>
                                    {
                                        r.Position = (uint) (startIndex + i + 1);
                                        PlayerEntity playerEntity = PlayerCache.Get(Convert.ToInt32(r.Login));
                                        r.Nickname = playerEntity.Nickname;
                                        r.Login = playerEntity.Login;
                                        return r;
                                    })
                                   .ToList();

                result = new PagedList<TopRankingEntry>(tempList) {VirtualCount = virtualCount, StartIndex = (int)startIndex};
            });

            return result;
        }

        List<RankingStats> IChallengeRankRepository.GetChallengeRankLeadersHavingTopXRanks(uint top, uint rankLimit)
        {
            List<RankingStats> rankStats = null;
            const string hqlQuery = "select PlayerId, count(*)" +
                                    "from ChallengeRankEntity " +
                                    "where Rank <= :RankLimit " +
                                    "group by PlayerId " +
                                    "order by count(*) desc";

            UseSession(session => 
            {
                List<object[]> list = session.CreateQuery(hqlQuery).SetInt32("RankLimit", (int)rankLimit).SetMaxResults((int)top).Enumerable<object[]>().ToList();
                rankStats = list.ConvertAll(r => new RankingStats { PlayerID = Convert.ToInt32(r[0]), Amount = Convert.ToUInt32(r[1]) });
            });

            rankStats.ForEach(r => r.Nickname = PlayerCache.Get(r.PlayerID).Nickname);

            return rankStats;
        }

        #endregion
    }
}