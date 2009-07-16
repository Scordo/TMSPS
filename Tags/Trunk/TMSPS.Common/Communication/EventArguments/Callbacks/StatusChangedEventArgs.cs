using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class StatusChangedEventArgs : EventArgsBase<StatusChangedEventArgs>
    {
        [RPCParam(0)]
        public int StatusCode { get; set; }
        [RPCParam(1)]
        public string StatusName { get; set; }
    }
}