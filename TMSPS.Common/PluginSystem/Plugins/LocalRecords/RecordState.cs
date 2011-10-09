namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class RecordState
    {
        public bool Improved 
        {
            get
            {
                return !PrevTimeOrScore.HasValue || CurrentTimeOrScore < PrevTimeOrScore;
            }
        }
        public int ChallengeId { get; set; }
        public int? PrevTimeOrScore { get; set; }
        public int CurrentTimeOrScore { get; set; }
        public uint? PrevPosition { get; set; }
        public uint CurrentPosition { get; set; }
    }
}
