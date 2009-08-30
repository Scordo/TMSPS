using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ReadOnlyCollection<ITMSPSPlugin> Plugins { get; private set; }

        private ushort LastPluginID { get; set; }

        #endregion

        #region Events

        public event EventHandler<ShutdownRequestedEventArgs> ShutdownRequested;

        #endregion

        #region Constructor

        public PluginHostContext(TrackManiaRPCClient client, ServerInfo serverInfo, Credentials credentials, MessageStyles messageStyles, MessageConstants messageConstants, List<ITMSPSPlugin> plugins)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (serverInfo == null)
                throw new ArgumentNullException("serverInfo");

            if (messageStyles == null)
                throw new ArgumentNullException("messageStyles");

            if (messageConstants == null)
                throw new ArgumentNullException("messageConstants");

            if (plugins == null)
                throw new ArgumentNullException("plugins");

            RPCClient = client;
            ServerInfo = serverInfo;
            Credentials = credentials;
            ValueStore = new ValueStore();
            Culture = new CultureInfo("en-us");
            MessagStyles = messageStyles;
            MessageConstants = messageConstants;
            CorePlugin = (TMSPSCorePlugin)plugins[0];
            LastPluginID = 0;
            Plugins = plugins.AsReadOnly();
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

        public void RequestShutdown(object caller, string reason)
        {
            if (ShutdownRequested != null)
                ShutdownRequested(caller, new ShutdownRequestedEventArgs(reason));
        }

        #endregion
    }

    public class ShutdownRequestedEventArgs : EventArgs
    {
        public string Reason { get; private set; }

        public ShutdownRequestedEventArgs(string reason)
        {
            Reason = reason;
        }
    }
}