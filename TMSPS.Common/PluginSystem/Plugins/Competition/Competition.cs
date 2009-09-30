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

        public Competition(string leader, int roundLimit)
        {
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
            if (Started)
                throw new InvalidOperationException("Joining a running competition is not allowed");

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
            List<Competitor> orderedCompetitors = new List<Competitor>();
            foreach(PlayerRank rank in rankings.OrderBy(r => r.Rank))
            {
                foreach (Competitor competitor in Competitors)
                {
                    if (rank.Login == competitor.Login)
                    {
                        orderedCompetitors.Add(competitor);
                        break;
                    }
                }
            }

            int[] points = new [] {10, 7, 5, 2, 1};

            for (int i = 0; i < orderedCompetitors.Count; i++ )
            {
                if (i >= points.Length)
                    break;

                orderedCompetitors[i].Score += points[i];
            }
            
            DrivenRounds++;
        }

        public IEnumerable<Competitor> GetRanking()
        {
            return Competitors.OrderByDescending(c => c.Score);
        }
    }
}