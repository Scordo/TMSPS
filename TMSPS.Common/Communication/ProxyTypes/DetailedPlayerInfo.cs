using System.Collections.Generic;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class DetailedPlayerInfo : PlayerInfoBase
    {
        [RPCParam("Path")]
        public string Path { get; set; }

        [RPCParam("Language")]
        public string Language { get; set; }

        [RPCParam("ClientVersion")]
        public string ClientVersion { get; set; }

        [RPCParam("IPAddress")]
        public string IPAddress { get; set; }

        [RPCParam("DownloadRate")]
        public int DownloadRate { get; set; }

        [RPCParam("UploadRate")]
        public int UploadRate { get; set; }

        [RPCParam("IsReferee")]
        public bool IsReferee { get; set; }

        [RPCParam("Avatar")]
        public Avatar Avatar { get; set; }

        [RPCParam("Skins")]
        public List<Skin> Skins { get; set; }

        [RPCParam("LadderStats")]
        public LadderStats LadderStats { get; set; }

        [RPCParam("HoursSinceZoneInscription")]
        public int HoursSinceZoneInscription { get; set; }

        [RPCParam("OnlineRights")]
        public int OnlineRights { get; set; }
    }
}
