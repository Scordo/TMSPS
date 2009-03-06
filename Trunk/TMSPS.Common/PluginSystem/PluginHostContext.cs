using System;
using System.Globalization;
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
        public CultureInfo Culture { get; private set; }
        public MessageStyles MessagStyles { get; private set; }
        public MessageConstants MessageConstants { get; private set; }

        public PluginHostContext(TrackManiaRPCClient client, ServerInfo serverInfo, Credentials credentials, MessageStyles messageStyles, MessageConstants messageConstants)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (serverInfo == null)
                throw new ArgumentNullException("serverInfo");

            if (messageStyles == null)
                throw new ArgumentNullException("messageStyles");

            if (messageConstants == null)
                throw new ArgumentNullException("messageConstants");

            RPCClient = client;
            ServerInfo = serverInfo;
        	Credentials = credentials;
            ValueStore = new ValueStore();
            Culture = new CultureInfo("en-us");
            MessagStyles = messageStyles;
            MessageConstants = messageConstants;
        }
    }
}