﻿using System;
using System.Collections.Generic;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    [Obsolete("Please use EndChallengeEventArgs")]
    public class EndRaceEventArgs : EventArgsBase<EndRaceEventArgs>
    {
        [RPCParam(0)]
        public List<PlayerRank> Rankings { get; set; }
        [RPCParam(1)]
        public ChallengeInfo Challenge { get; set; }
    }
}