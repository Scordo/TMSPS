using System;
using TMSPS.Core.Communication;
using TMSPS.TRC.BL.Configuration;

namespace TMSPS.TRC.BL.Wpf
{
    public class ServerControlDataContext
    {
        public ServerInfo ServerInfo { get; private set; }
        public TrackManiaRPCClient RPCClient { get; private set; }

        public ServerControlDataContext(ServerInfo serverInfo)
        {
            if (serverInfo == null)
                throw new ArgumentNullException("serverInfo");

            ServerInfo = serverInfo;
            RPCClient = new TrackManiaRPCClient(serverInfo.Address, serverInfo.XmlRpcPort);
        }
    }
}