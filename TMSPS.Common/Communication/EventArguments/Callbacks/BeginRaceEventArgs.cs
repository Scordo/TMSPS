using System;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    [Obsolete("Please use BeginChallengeEventArgs")]
    public class BeginRaceEventArgs : EventArgsBase<BeginRaceEventArgs>
    {
        [RPCParam(0)]
        public ChallengeInfo ChallengeInfo { get; set; }
    }
}
