using FluentNHibernate.Mapping;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.EntityMappings
{
    public class ChallengeEntityMap: ClassMap<ChallengeEntity>
    {
        public ChallengeEntityMap()
        {
            Id(c => c.Id).GeneratedBy.Identity();
            Map(c => c.UniqueId).Length(27);
            Map(c => c.Name).Length(100);
            Map(c => c.Author).Length(50);
            Map(c => c.Environment).Length(20);
            Map(c => c.Races);
            Map(c => c.Created).ReadOnly();
            Map(c => c.LastChanged).Nullable();
            Table("Challenge");
        }
    }
}