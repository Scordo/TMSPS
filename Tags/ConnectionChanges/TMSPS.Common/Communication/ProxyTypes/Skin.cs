using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class Skin : IDump
    {
        [RPCParam("Environnement")]
        public string Environnement { get; set; }

        [RPCParam("PackDesc")]
        public SkinDetails Details { get; set; }
    }
}
