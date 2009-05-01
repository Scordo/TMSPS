using System.Collections.Generic;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class EndChallengeEventArgs : EventArgsBase<EndChallengeEventArgs>
    {
        [RPCParam(0)]
        public List<PlayerRank> Rankings { get; set; }

        [RPCParam(1)]
        public ChallengeInfo Challenge { get; set; }

        [RPCParam(2)]
        public bool WasWarmUp { get; set; }

        [RPCParam(3)]
        public bool MatchContinuesOnNextChallenge { get; set; }
    }
}