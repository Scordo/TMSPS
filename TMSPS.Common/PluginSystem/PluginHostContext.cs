using System;
using System.Globalization;
using TMSPS.Core.Communication;
using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.Core.PluginSystem
{
    public class PluginHostContext
    {
        #region Members

        private readonly object _pluginIDLockObject = new object();

        #endregion

        #region Properties

        public Credentials Credentials { get; internal set; } 
        public TrackManiaRPCClient RPCClient { get; private set; }
        public ServerInfo ServerInfo { get; internal set; }
        public ValueStore ValueStore { get; private set; }
        public CultureInfo Culture { get; private set; }
        public MessageStyles MessagStyles { get; private set; }
        public MessageConstants MessageConstants { get; private set; }
        private ushort LastPluginID { get; set; }

        #endregion

        #region Constructor

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
            LastPluginID = 0;
        }

        #endregion

        #region Public Methods

        public ushort GetUniquePluginID()
        {
            lock (_pluginIDLockObject)
            {
                if (LastPluginID == 1023)
                    throw new InvalidOperationException("No more plugin ids available. Only 1023 active plugins are supported!");

                LastPluginID++;
                return LastPluginID;
            }
        }

        #endregion
    }
}