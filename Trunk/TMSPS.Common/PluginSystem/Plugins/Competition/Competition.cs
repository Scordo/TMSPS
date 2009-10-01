using System;
using System.Collections.Generic;
using TMSPS.Core.Communication.ProxyTypes;
using System.Linq;

namespace TMSPS.Core.PluginSystem.Plugins.Competition
{
    public class Competition
    {
        public string Leader { get; private set; }
        public List<Competitor> Competitors { get; private set; }
        public bool Started { get; private set; }
        public string Name { get; private set; }
        public int RoundLimit { get; private set; }
        public int DrivenRounds { get; private set; }
        public bool Finished { get { return DrivenRounds >= RoundLimit; } }
        public string Password { get; private set; }
        public bool IsPasswordProtected { get { return Password != null; } }

        public Competition(string leader, int roundLimit, string password)
        {
            Password = password;
            RoundLimit = roundLimit;
            DrivenRounds = 0;
            Name = leader;
            Leader = leader;
            Started = false;
            Competitors = new List<Competitor>{new Competitor(leader)};
        }

        public void Start()
        {
            if (Started)
                throw new InvalidOperationException("Competition already started");

            Started = true;
        }

        public void Join(string login)
        {
            if (IsTakenPart(login))
                throw new InvalidOperationException(string.Format("{0} is already taking part in this competition", login));

            Competitors.Add(new Competitor(login));
        }

        public bool IsTakenPart(string login)
        {
            return Competitors.Exists(c => c.Login == login);
        }

        public void Leave(string login)
        {
            if (!IsTakenPart(login))
                throw new InvalidOperationException(string.Format("{0} is not taking part in this competition", login));

            Competitors.RemoveAll(c => c.Login == login);
        }

        public void UpdateWithEndRaceResult(List<PlayerRank> rankings)
        {
            List<PlayerRank> orderedCompetitorRanks = new List<PlayerRank>();
            foreach(PlayerRank rank in rankings.OrderBy(r => r.Rank))
            {
                foreach (Competitor competitor in Competitors)
                {
                    if (rank.Login == competitor.Login)
                    {
                        orderedCompetitorRanks.Add(rank);
                        break;
                    }
                }
            }

            int[] points = new [] {10, 7, 5, 2, 1};

            for (int i = 0; i < orderedCompetitorRanks.Count; i++ )
            {
                if (i >= points.Length)
                    break;

                PlayerRank competitorRank = orderedCompetitorRanks[i];
                if (competitorRank.BestTime <= 0)
                    break;

                Competitor competitor = Competitors.Find(c => c.Login == competitorRank.Login);
                if (competitor == null)
                    continue;

                competitor.Score += points[i];
            }
            
            DrivenRounds++;
        }

        public IEnumerable<Competitor> GetRanking()
        {
            return Competitors.OrderByDescending(c => c.Score);
        }
    }
}