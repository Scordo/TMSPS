using FluentNHibernate.Mapping;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.EntityMappings
{
    public class RatingMap : ClassMap<RatingEntity>
    {
        public RatingMap()
        {
            Id(r => r.Id).GeneratedBy.Identity();
            Map(r => r.PlayerId);
            Map(r => r.ChallengeId);
            Map(r => r.Value);
            Map(r => r.Created).ReadOnly();
            Map(r => r.LastChanged).Nullable();
            Table("Rating");
        }
    }
}