using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class ChallengeListSingleInfo : IDump
    {
        [RPCParam("Name")]
        public string Name { get; set; }

        [RPCParam("UId")]
        public string UId { get; set; }
        
        [RPCParam("FileName")]
        public string FileName { get; set; }
        
        [RPCParam("Author")]
        public string Author { get; set; }

        [RPCParam("Environnement")]
        public string Environnement { get; set; }

        [RPCParam("GoldTime")]
        public int GoldTime { get; set; }

        [RPCParam("CopperPrice")]
        public int CopperPrice { get; set; }
    }
}