using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class BanEntry : IDump
    {
        [RPCParam("Login")]
        public string Login { get; set; }

        [RPCParam("ClientName")]
        public string ClientName { get; set; }

        [RPCParam("IPAddress")]
        public string IPAddress { get; set; }
    }
}
