namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities
{
    public class ServerRankEntity
    {
        public virtual int PlayerId { get; set; }
        public virtual int? Rank { get; set; }
        public virtual int? AverageRank { get; set; }
        public virtual int RecordsCount { get; set; }
        public virtual int ChallengesCount { get; set; }
        public virtual decimal? Score { get; set; }
    }
}
