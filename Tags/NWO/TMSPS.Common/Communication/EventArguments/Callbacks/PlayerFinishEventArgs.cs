using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class PlayerFinishEventArgs : EventArgsBase<PlayerFinishEventArgs>
    {
        [RPCParam(0)]
        public int PlayerID { get; set; }
        [RPCParam(1)]
        public string Login { get; set; }
        [RPCParam(2)]
        public int TimeOrScore { get; set; }
    }
}