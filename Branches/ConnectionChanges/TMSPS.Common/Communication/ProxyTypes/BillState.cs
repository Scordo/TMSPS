using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class BillState : IDump
    {
        [RPCParam("State")]
        public int State { get; set; }

        [RPCParam("StateName")]
        public string StateName { get; set; }

        [RPCParam("TransactionId")]
        public int TransactionId { get; set; }
    }
}
