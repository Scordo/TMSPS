using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class PlayerIncoherenceEventArgs : EventArgsBase<PlayerIncoherenceEventArgs>
    {
        [RPCParam(0)]
        public int PlayerID { get; set; }
        [RPCParam(1)]
        public string Login { get; set; }
    }
}