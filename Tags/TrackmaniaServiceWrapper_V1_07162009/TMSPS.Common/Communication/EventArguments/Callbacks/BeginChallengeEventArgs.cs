using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class BeginChallengeEventArgs : EventArgsBase<BeginChallengeEventArgs>
    {
        [RPCParam(0)]
        public ChallengeInfo ChallengeInfo { get; set; }

        [RPCParam(1)]
        public bool IsWarmUp { get; set; }

        [RPCParam(2)]
        public bool IsMatchContinuation { get; set; }
    }
}