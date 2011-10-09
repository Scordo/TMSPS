using FluentNHibernate.Mapping;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.EntityMappings
{
    public class RecordMap : ClassMap<RecordEntity>
    {
        public RecordMap()
        {
            Id(r => r.Id).GeneratedBy.Identity();
            Map(r => r.PlayerId);
            Map(r => r.ChallengeId);
            Map(r => r.TimeOrScore);
            Map(r => r.Created);
            References(r => r.Player)
                .Column("PlayerId")
                .ReadOnly()
                .LazyLoad(Laziness.NoProxy);
            References(r => r.Challenge)
                .Column("ChallengeId")
                .ReadOnly()
                .LazyLoad(Laziness.NoProxy);
            Table("Record");
        }
    }
}