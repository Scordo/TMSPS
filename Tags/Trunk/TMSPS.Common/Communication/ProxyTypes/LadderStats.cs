using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class LadderStats : IDump
    {
        [RPCParam("LastMatchScore")]
        public double LastMatchScore { get; set; }

        [RPCParam("NbrMatchWins")]
        public int NbrMatchWins { get; set; }

        [RPCParam("NbrMatchDraws")]
        public int NbrMatchDraws { get; set; }

        [RPCParam("NbrMatchLosses")]
        public int NbrMatchLosses { get; set; }

        [RPCParam("TeamName")]
        public string TeamName { get; set; }

        [RPCParam("PlayerRankings")]
        public List<PlayerRanking> PlayerRankings { get; set; }

        // dont know the structure --> hope to find information about it
        [RPCParam("TeamRankings")]
        public List<string> TeamRankings { get; set; }
    }
}