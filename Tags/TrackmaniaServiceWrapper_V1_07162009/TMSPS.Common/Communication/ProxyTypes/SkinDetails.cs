using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class SkinDetails : IDump
    {
        [RPCParam("FileName")]
        public string FileName { get; set; }

        [RPCParam("Checksum")]
        public string Checksum { get; set; }
    }
}
