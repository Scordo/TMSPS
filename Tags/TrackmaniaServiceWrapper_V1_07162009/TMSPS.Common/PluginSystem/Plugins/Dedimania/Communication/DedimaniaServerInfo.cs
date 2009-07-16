namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaServerInfo
    {
        public string SrvName { get; set; }
        public string Comment { get; set; }
        public bool Private { get; set; }
        public string SrvIP { get; set; }
        public int SrvPort { get; set; }
        public int XmlrpcPort { get; set; }
        public int NumPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int NumSpecs { get; set; }
        public int MaxSpecs { get; set; }
        public int LadderMode { get; set; }
        public string NextFiveUID { get; set; }

        public DedimaniaServerInfo()
        {

        }

        public DedimaniaServerInfo(string srvName, string comment, bool isPrivate, string srvIP, int srvPort, int xmlrpcPort, int numPlayers, int maxPlayers, int numSpecs, int maxSpecs, int ladderMode, string nextFiveUID)
        {
            SrvName = srvName;
            Comment = comment;
            Private = isPrivate;
            SrvIP = srvIP;
            SrvPort = srvPort;
            XmlrpcPort = xmlrpcPort;
            NumPlayers = numPlayers;
            MaxPlayers = maxPlayers;
            NumSpecs = numSpecs;
            MaxSpecs = maxSpecs;
            LadderMode = ladderMode;
            NextFiveUID = nextFiveUID;
        }
    }
}