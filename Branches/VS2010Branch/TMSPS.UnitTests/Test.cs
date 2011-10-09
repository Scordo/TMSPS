using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.UnitTests
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void TestIt()
        {
            PlayerCache.Init(1000);
            IChallengeRankRepository cr = RepositoryFactory.Get<IChallengeRankRepository>();

            List<RankingStats> ranks = cr.GetChallengeRankLeadersHavingTopXRanks(5, 50);

            //int rankLimit = 50;
            //int top = 5;

            ////var first = session.Query<ChallengeRankEntity>().Where(cr => cr.Rank <= rankLimit).GroupBy(cr => cr.PlayerId, cr => cr.PlayerId).Select(g => new { A = g.Key }).Take((int)top).ToList(); // .Select(g => new RankingStats { PlayerID = g.Key, Amount = (uint)g.Count() })

            //string query = "select PlayerId as PlayerId, count(*) as Amount " +
            //               "from ChallengeRankEntity " +
            //               "where Rank <= :RankLimit " +
            //               "group by PlayerId " +
            //               "order by count(*) desc";

            //List<object[]> list = session.CreateQuery(query).SetInt32("RankLimit", rankLimit).SetMaxResults(top).Enumerable<object[]>().ToList();

            //List<IGrouping<int, int>> rankStats = new List<IGrouping<int,int>>(first);
            //rankStats.ForEach(r => r.Nickname = PlayerCache.Instance.Get(r.PlayerID).Nickname);

            //return rankStats;
        }
    }
}