using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities
{
    public class ChallengeEntity
    {
        public virtual int? Id { get; private set; }
        public virtual string UniqueId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Author { get; set; }
        public virtual string Environment { get; set; }
        public virtual int Races { get; set; }
        public virtual DateTime? Created { get; private set; }
        public virtual DateTime? LastChanged { get; set; }

        public virtual ChallengeEntity Clone()
        {
            return (ChallengeEntity) MemberwiseClone();
        }
    }
}
