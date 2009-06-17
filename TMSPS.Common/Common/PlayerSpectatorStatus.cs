namespace TMSPS.Core.Common
{
    public class PlayerSpectatorStatus : IDump
    {
        public bool IsSpectator { get; set; }
        public bool IsTemporarySpectator { get; set; }
        public bool IsPureSpectator { get; set; }
        public bool IsAutoTarget { get; set; }
        public int CurrentPlayerTargetID { get; set; }
        public bool IsPCurrentPlayerTargetIDValid { get { return CurrentPlayerTargetID != 0 && CurrentPlayerTargetID != 255; } }

        public static PlayerSpectatorStatus Parse(int spectatorStatus)
        {
            PlayerSpectatorStatus result = new PlayerSpectatorStatus();
            
            result.CurrentPlayerTargetID = spectatorStatus/10000;
            spectatorStatus -= result.CurrentPlayerTargetID * 10000;

            result.IsAutoTarget = (spectatorStatus / 1000) > 0;
            spectatorStatus -= (result.IsAutoTarget ? 1 : 0) *  1000;

            result.IsPureSpectator = (spectatorStatus / 100) > 0;
            spectatorStatus -= (result.IsPureSpectator ? 1 : 0) * 100;

            result.IsTemporarySpectator = (spectatorStatus / 10) > 0;
            spectatorStatus -= (result.IsTemporarySpectator ? 1 : 0) * 10;

            result.IsSpectator = (spectatorStatus > 0);

            return result;
        }
    }
}