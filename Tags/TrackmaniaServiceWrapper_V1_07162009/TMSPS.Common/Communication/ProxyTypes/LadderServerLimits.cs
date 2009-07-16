using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class LadderServerLimits : IDump
    {
        [RPCParam("LadderServerLimitMin")]
        public double LadderServerLimitMin { get; set; }

        [RPCParam("LadderServerLimitMax")]
        public double LadderServerLimitMax { get; set; }
    }
}
