using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class BillUpdatedEventArgs : EventArgsBase<BillUpdatedEventArgs>
    {
        [RPCParam(0)]
        public int BillID { get; set; }
        [RPCParam(1)]
        public int State { get; set; }
        [RPCParam(2)]
        public string StateName { get; set; }
        [RPCParam(3)]
        public int TransactionID { get; set; }
    }
}