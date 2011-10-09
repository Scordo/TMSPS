using FluentNHibernate.Mapping;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.EntityMappings
{
    public class ServerRankMap : ClassMap<ServerRankEntity>
    {
        public ServerRankMap()
        {
            Id(r => r.PlayerId).GeneratedBy.Assigned();
            Map(r => r.Rank).Nullable();
            Map(r => r.AverageRank).Nullable();
            Map(r => r.RecordsCount).Nullable();
            Map(r => r.ChallengesCount).Nullable();
            Map(r => r.Score).Nullable();
            Table("ServerRank");
        }
    }
}
