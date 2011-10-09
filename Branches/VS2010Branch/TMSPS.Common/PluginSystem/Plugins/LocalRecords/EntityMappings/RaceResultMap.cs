using FluentNHibernate.Mapping;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.EntityMappings
{
    public class RaceResultMap : ClassMap<RaceResultEntity>
    {
        public RaceResultMap()
        {
            Id(r => r.Id).GeneratedBy.Identity();
            Map(r => r.PlayerId);
            Map(r => r.ChallengeId);
            Map(r => r.Position);
            Map(r => r.PlayersCount);
            Map(r => r.Created).ReadOnly();
            Table("RaceResult");
        }
    }
}