using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class EchoEventArgs : EventArgsBase<EchoEventArgs>
    {
        [RPCParam(0)]
        public string Internal { get; set; }
        [RPCParam(1)]
        public string Public { get; set; }
    }
}