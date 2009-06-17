using TMSPS.Core.Communication.ProxyTypes;

namespace TMSPS.Core.Common
{
    public class PlayerFlags : IDump
    {
        public bool HasPlayerSlot { get; set; }
        public bool IsServer { get; set; }
        public bool IsManagedByAnOtherServer { get; set; }
        public bool IsUsingStereoscopy { get; set; }
        public bool IsPodiumReady { get; set; }
        public bool IsReferee { get; set; }
        public ForceSpectatorState ForceSpectator { get; set; }

        public static PlayerFlags Parse(int spectatorStatus)
        {
            PlayerFlags result = new PlayerFlags();

            result.HasPlayerSlot = (spectatorStatus / 1000000) > 0;
            spectatorStatus -= (result.HasPlayerSlot ? 1 : 0) * 1000000;

            result.IsServer = (spectatorStatus / 100000) > 0;
            spectatorStatus -= (result.IsServer ? 1 : 0) * 100000;

            result.IsManagedByAnOtherServer = (spectatorStatus / 10000) > 0;
            spectatorStatus -= (result.IsManagedByAnOtherServer ? 1 : 0) * 10000;

            result.IsUsingStereoscopy = (spectatorStatus / 1000) > 0;
            spectatorStatus -= (result.IsUsingStereoscopy ? 1 : 0) * 1000;

            result.IsPodiumReady = (spectatorStatus / 100) > 0;
            spectatorStatus -= (result.IsPodiumReady ? 1 : 0) * 100;

            result.IsReferee = (spectatorStatus / 10) > 0;
            spectatorStatus -= (result.IsReferee ? 1 : 0) * 10;

            result.ForceSpectator = (ForceSpectatorState) spectatorStatus;

            return result;
        }
    }
}
