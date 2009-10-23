using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class ServerOptions : IDump
    {
        [RPCParam("Name")]
        public string Name { get; set; }

        [RPCParam("Comment")]
        public string Comment { get; set; }

        [RPCParam("Password")]
        public string Password { get; set; }

        [RPCParam("PasswordForSpectator")]
        public string PasswordForSpectator { get; set; }

        [RPCParam("CurrentMaxPlayers")]
        public int CurrentMaxPlayers { get; set; }

        [RPCParam("NextMaxPlayers")]
        public int NextMaxPlayers { get; set; }

        [RPCParam("CurrentMaxSpectators")]
        public int CurrentMaxSpectators { get; set; }

        [RPCParam("NextMaxSpectators")]
        public int NextMaxSpectators { get; set; }

        [RPCParam("IsP2PUpload")]
        public bool IsP2PUpload { get; set; }

        [RPCParam("IsP2PDownload")]
        public bool IsP2PDownload { get; set; }

        [RPCParam("CurrentLadderMode")]
        public int CurrentLadderMode { get; set; }

        [RPCParam("NextLadderMode")]
        public int NextLadderMode { get; set; }

        [RPCParam("CurrentVehicleNetQuality")]
        public int CurrentVehicleNetQuality { get; set; }

        [RPCParam("NextVehicleNetQuality")]
        public int NextVehicleNetQuality { get; set; }

        [RPCParam("CurrentCallVoteTimeOut")]
        public int CurrentCallVoteTimeOut { get; set; }

        [RPCParam("NextCallVoteTimeOut")]
        public int NextCallVoteTimeOut { get; set; }

        [RPCParam("CallVoteRatio")]
        public double CallVoteRatio { get; set; }

        [RPCParam("AllowChallengeDownload")]
        public bool AllowChallengeDownload { get; set; }

        [RPCParam("AutoSaveReplays")]
        public bool AutoSaveReplays { get; set; }
    }
}
