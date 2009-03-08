using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class GameInfo : IDump
    {
        [RPCParam("GameMode")]
        public int GameMode { get; set; }

        [RPCParam("ChatTime")]
        public int ChatTime { get; set; }

        [RPCParam("NbChallenge")]
        public int NbChallenge { get; set; }

        [RPCParam("RoundsPointsLimit")]
        public int RoundsPointsLimit { get; set; }

        [RPCParam("RoundsUseNewRules")]
        public bool RoundsUseNewRules { get; set; }

        [RPCParam("RoundsForcedLaps")]
        public int RoundsForcedLaps { get; set; }

        [RPCParam("TimeAttackLimit")]
        public int TimeAttackLimit { get; set; }

        [RPCParam("TimeAttackSynchStartPeriod")]
        public int TimeAttackSynchStartPeriod { get; set; }

        [RPCParam("TeamPointsLimit")]
        public int TeamPointsLimit { get; set; }

        [RPCParam("TeamMaxPoints")]
        public int TeamMaxPoints { get; set; }

        [RPCParam("TeamUseNewRules")]
        public bool TeamUseNewRules { get; set; }

        [RPCParam("LapsNbLaps")]
        public int LapsNbLaps { get; set; }

        [RPCParam("LapsTimeLimit")]
        public int LapsTimeLimit { get; set; }

        [RPCParam("FinishTimeout")]
        public int FinishTimeout { get; set; }
    }
}