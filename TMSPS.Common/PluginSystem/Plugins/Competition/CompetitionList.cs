using System.Collections.Generic;

namespace TMSPS.Core.PluginSystem.Plugins.Competition
{
    public class CompetitionList : List<Competition>
    {
        public bool ContainsCompetitor(string login)
        {
            return Exists(competition => competition.Competitors.Exists(competitor => competitor.Login == login));
        }

        public bool IsLeaderOfACompetition(string login)
        {
            return Exists(competition => competition.Leader == login);
        }

        public Competition GetByName(string competitionName)
        {
            return Find(c => c.Name == competitionName);
        }

        public Competition GetCompetitionByLogin(string login)
        {
            return Find(competition => competition.Competitors.Exists(competitor => competitor.Login == login));
        }

        public IEnumerable<Competition> GetStartedCompetitions()
        {
            foreach (Competition competition in this)
            {
                if (competition.Started)
                    yield return competition;
            }
        }

        public IEnumerable<Competition> GetFinishedCompetitions()
        {
            foreach (Competition competition in this)
            {
                if (competition.Finished)
                    yield return competition;
            }
        }
    }
}
