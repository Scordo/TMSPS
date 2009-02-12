using System;
using TMSPS.Core.Communication;
using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.Core.PluginSystem
{
    public class PluginHostContext
    {
		public Credentials Credentials { get; internal set; } 
        public TrackManiaRPCClient RPCClient { get; private set; }
        public ServerInfo ServerInfo { get; internal set; }
        public ValueStore ValueStore { get; private set; }

        public PluginHostContext(TrackManiaRPCClient client, ServerInfo serverInfo, Credentials credentials)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (serverInfo == null)
                throw new ArgumentNullException("serverInfo");

            RPCClient = client;
            ServerInfo = serverInfo;
        	Credentials = credentials;
            ValueStore = new ValueStore();
        }
    }
}