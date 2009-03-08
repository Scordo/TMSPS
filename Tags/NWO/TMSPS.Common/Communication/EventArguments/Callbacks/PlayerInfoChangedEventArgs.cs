using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class PlayerInfoChangedEventArgs : EventArgsBase<PlayerInfoChangedEventArgs>
    {
        [RPCParam(0)]
        public PlayerInfo PlayerInfo { get; set; }
    }

    public class PlayerInfo : IDump
    {
        [RPCParam("Login")]
        public string Login { get; set; }
        [RPCParam("NickName")]
        public string NickName { get; set; }
        [RPCParam("PlayerId")]
        public int PlayerID { get; set; }
        [RPCParam("TeamId")]
        public int TeamID { get; set; }
        [RPCParam("SpectatorStatus")]
        public int SpectatorStatus { get; set; }
        [RPCParam("LadderRanking")]
        public int LadderRanking { get; set; }
        [RPCParam("Flags")]
        public int Flags { get; set; }
    }
}