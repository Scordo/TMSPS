using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class TunnelDataReceivedEventArgs : EventArgsBase<TunnelDataReceivedEventArgs>
    {
        [RPCParam(0)]
        public int PlayerID { get; set; }
        [RPCParam(1)]
        public string Login { get; set; }
        [RPCParam(2)]
        public byte[] Data { get; set; }
    }
}