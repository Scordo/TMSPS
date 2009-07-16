using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class PlayerInfo : PlayerInfoBase
    {
        [RPCParam("LadderRanking")]
        public int LadderRanking { get; set; }
    }
}