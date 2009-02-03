using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class PlayerRanking : IDump
    {
        [RPCParam("Path")]
        public string Path { get; set; }

        [RPCParam("Score")]
        public double Score { get; set; }

        [RPCParam("Ranking")]
        public int Ranking { get; set; }

        [RPCParam("TotalCount")]
        public int TotalCount { get; set; }
    }
}