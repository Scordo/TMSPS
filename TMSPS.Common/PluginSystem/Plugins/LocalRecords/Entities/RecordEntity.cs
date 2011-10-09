using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities
{
    public class RecordEntity
    {
        public virtual int? Id { get; private set; }
        public virtual int PlayerId { get; set; }
        public virtual PlayerEntity Player { get; private set; }
        public virtual int ChallengeId { get; set; }
        public virtual ChallengeEntity Challenge { get; private set; }
        public virtual int TimeOrScore { get; set; }
        public virtual DateTime? Created { get; set; }
    }
}
