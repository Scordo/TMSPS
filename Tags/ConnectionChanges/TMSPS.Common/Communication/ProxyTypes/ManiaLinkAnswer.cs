using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class ManiaLinkAnswer : IDump
    {
        [RPCParam("Login")]
        public string Login { get; set; }

        [RPCParam("PlayerId")]
        public int PlayerId { get; set; }

        [RPCParam("Result")]
        public int Result { get; set; }
    }
}