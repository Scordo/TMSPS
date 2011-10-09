using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities
{
    public class RaceResultEntity
    {
        public virtual int? Id { get; private set; }
        public virtual int PlayerId { get; set; }
        public virtual int ChallengeId { get; set; }
        public virtual short Position { get; set; }
        public virtual short PlayersCount { get; set; }
        public virtual DateTime? Created { get; private set; }
    }
}