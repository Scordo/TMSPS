using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class CNPair<T> : IDump
    {
        [RPCParam("CurrentValue")]
        public T CurrentValue { get; set; }

        [RPCParam("NextValue")]
        public T NextValue { get; set; }
    }
}