using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class LoginResponse : IDump
    {
        [RPCParam("Login")]
        public string Login { get; set; }
    }
}
