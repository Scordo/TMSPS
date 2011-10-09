using FluentNHibernate.Mapping;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.EntityMappings
{
    public class ChallengeRankMap : ClassMap<ChallengeRankEntity>
    {
        public ChallengeRankMap()
        {
            CompositeId()
                .KeyProperty(c => c.PlayerId)
                .KeyProperty(c => c.ChallengeId);

            References(c => c.Player)
                .Column("PlayerId")
                .ReadOnly()
                .Fetch.Join();

            Map(r => r.Rank);
            Table("ChallengeRank");
        }
    }
}