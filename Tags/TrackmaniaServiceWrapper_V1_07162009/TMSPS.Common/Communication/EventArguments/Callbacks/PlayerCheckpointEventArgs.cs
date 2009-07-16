using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class PlayerCheckpointEventArgs : EventArgsBase<PlayerCheckpointEventArgs>
    {
        [RPCParam(0)]
        public int PlayerID { get; set; }
        [RPCParam(1)]
        public string Login { get; set; }
        [RPCParam(2)]
        public int TimeOrScore { get; set; }
        [RPCParam(3)]
        public int CurLap { get; set; }
        [RPCParam(4)]
        public int CheckpointIndex { get; set; }
    }
}