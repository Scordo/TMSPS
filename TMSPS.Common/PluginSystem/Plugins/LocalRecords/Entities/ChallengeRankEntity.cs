namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities
{
    public class ChallengeRankEntity
    {
        public virtual int PlayerId { get; set; }
        public virtual PlayerEntity Player { get; set; }
        public virtual int ChallengeId { get; set; }
        public virtual int Rank { get; set; }

        public override bool Equals(object instance)
        {
            if (instance == null)
                return false;

            ChallengeRankEntity rank = instance as ChallengeRankEntity;

            if (rank == null)
                return false;

            return rank.ChallengeId == ChallengeId && rank.PlayerId == PlayerId;
        }

        public override int GetHashCode()
        {
            return string.Format("{0}|{1}", PlayerId, ChallengeId).GetHashCode();
        }
    }
}
