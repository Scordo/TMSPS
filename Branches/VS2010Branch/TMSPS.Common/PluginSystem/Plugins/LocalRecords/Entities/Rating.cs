using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities
{
    public class RatingEntity
    {
        public virtual int? Id { get; private set; }
        public virtual int PlayerId { get; set; }
        public virtual int ChallengeId { get; set; }
        public virtual short Value { get; set; }
        public virtual DateTime? Created { get; private set; }
        public virtual DateTime? LastChanged { get; set; }
    }
}
