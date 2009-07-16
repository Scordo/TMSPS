using System;
using System.Globalization;
using TMSPS.Core.Communication;
using TMSPS.Core.PluginSystem.Configuration;
using TMSPS.Core.PluginSystem.Plugins;

namespace TMSPS.Core.PluginSystem
{
    public class PluginHostContext
    {
        #region Members

        private readonly object _pluginIDLockObject = new object();
        private readonly PlayerSettingsStore _userSettings = new PlayerSettingsStore();

        #endregion

        #region Properties

        public Credentials Credentials { get; internal set; } 
        public TrackManiaRPCClient RPCClient { get; private set; }
        public ServerInfo ServerInfo { get; internal set; }
        public ValueStore ValueStore { get; private set; }
        public CultureInfo Culture { get; private set; }
        public MessageStyles MessagStyles { get; private set; }
        public MessageConstants MessageConstants { get; private set; }
        public PlayerSettingsStore PlayerSettings { get { return _userSettings; } }
        public TMSPSCorePlugin CorePlugin { get; private set; }

        private ushort LastPluginID { get; set; }

        #endregion

        #region Constructor

        public PluginHostContext(TrackManiaRPCClient client, ServerInfo serverInfo, Credentials credentials, MessageStyles messageStyles, MessageConstants messageConstants, TMSPSCorePlugin corePlugin)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (serverInfo == null)
                throw new ArgumentNullException("serverInfo");

            if (messageStyles == null)
                throw new ArgumentNullException("messageStyles");

            if (messageConstants == null)
                throw new ArgumentNullException("messageConstants");

            if (corePlugin == null)
                throw new ArgumentNullException("corePlugin");

            RPCClient = client;
            ServerInfo = serverInfo;
            Credentials = credentials;
            ValueStore = new ValueStore();
            Culture = new CultureInfo("en-us");
            MessagStyles = messageStyles;
            MessageConstants = messageConstants;
            CorePlugin = corePlugin;
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