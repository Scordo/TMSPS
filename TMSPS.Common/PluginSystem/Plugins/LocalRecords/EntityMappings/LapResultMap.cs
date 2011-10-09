using FluentNHibernate.Mapping;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.EntityMappings
{
    public class LapResultMap : ClassMap<LapResultEntity>
    {
        public LapResultMap()
        {
            Id(l => l.Id).GeneratedBy.Identity();
            Map(l => l.PlayerId);
            Map(l => l.ChallengeId);
            Map(l => l.TimeOrScore);
            Map(l => l.Created).ReadOnly();
            Table("LapResult");
        }
    }
}