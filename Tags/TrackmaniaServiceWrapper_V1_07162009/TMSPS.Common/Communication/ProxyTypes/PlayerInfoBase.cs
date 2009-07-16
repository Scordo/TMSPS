using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class PlayerInfoBase : IDump
    {
        [RPCParam("Login")]
        public string Login { get; set; }

        [RPCParam("NickName")]
        public string NickName { get; set; }

        [RPCParam("PlayerId")]
        public int PlayerId { get; set; }

        [RPCParam("TeamId")]
        public int TeamId { get; set; }

        [RPCParam("IsSpectator")]
        public bool IsSpectator { get; set; }

        [RPCParam("IsInOfficialMode")]
        public bool IsInOfficialMode { get; set; }
    }
}