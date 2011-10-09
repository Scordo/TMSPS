
using FluentNHibernate.Mapping;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.EntityMappings
{
    public class PlayerEntityMap : ClassMap<PlayerEntity>
    {
        public PlayerEntityMap()
        {
            Id(p => p.Id).GeneratedBy.Identity();
            Map(p => p.Login).Length(100);
            Map(p => p.Nickname).Length(100);
            Map(p => p.Wins);
            Map(p => p.TimePlayed);
            Map(p => p.Created).ReadOnly();
            Map(p => p.LastChanged).Nullable();
            Map(p => p.LastTimePlayedChanged);
            Table("Player");
        }
    }
}