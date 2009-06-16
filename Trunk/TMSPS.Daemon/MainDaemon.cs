using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TMSPS.Core.Common;
using TMSPS.Core.Communication;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.Logging;
using TMSPS.Core.PluginSystem;
using TMSPS.Core.PluginSystem.Configuration;
using TMSPS.Core.PluginSystem.Plugins;
using Version=TMSPS.Core.Communication.ProxyTypes.Version;

namespace TMSPS.Daemon
{
    internal class MainDaemon
    {
        #region Members

        private TrackManiaRPCClient _client;
        private List<ITMSPSPlugin> _plugins;
    	private readonly IUILogger _logger = new ConsoleUILogger("TMSPS", "DAEMON");

        #endregion

        #region Properties

    	private IUILogger Log
    	{
			get { return _logger; }
    	}

        private static string ApplicationDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        private static string ApplicationFileName
        {
            get { return Path.GetFileName(Assembly.GetExecutingAssembly().Location);}
        }

        private static string ApplicationConfigFileName
        {
            get { return ApplicationFileName + ".config"; }
        }

        private ConfigSettingsConfigurationSection ConfigSettings
        {
            get; set;
        }

        private List<ITMSPSPlugin> Plugins
        {
            get
            {
                if (_plugins == null)
                    _plugins = new List<ITMSPSPlugin>();

                return _plugins;
            }
            set { _plugins = value; }
        }

		private PluginHostContext HostContext
		{
			get; set;
		}

        #endregion

        #region Public Methods

        public void Start()
        {
            ReadConfigSettings();
            _client = new TrackManiaRPCClient(ConfigSettings.ServerAddress, ConfigSettings.ServerXMLRPCPort);
            _client.Connected += Connected;
            _client.ServerClosedConnection += ServerClosedConnection;
            _client.SocketError += SocketError;
            _client.ReadyForSendingCommands += ReadyForSendingCommands;

            _client.Connect();
        }

        public void Stop()
        {
            DisposePlugins(false);
            _client.Disconnect();

            _client.Connected -= Connected;
            _client.ServerClosedConnection -= ServerClosedConnection;
            _client.SocketError -= SocketError;
            _client.ReadyForSendingCommands -= ReadyForSendingCommands;
        }

        private void SocketError(object sender, Core.Communication.EventArguments.SocketErrorEventArgs e)
        {
            Log.ErrorToUI("Socket Error occured!");
            Log.Error(string.Format("Connection Error: {0}.", e.SocketError));
            DisposePlugins(true);
            _client.Connect();
        }

        private void ServerClosedConnection(object sender, EventArgs e)
        {
            Log.InfoToUI(string.Format("Lost connection to server {0} at port {1}.", ConfigSettings.ServerAddress, ConfigSettings.ServerXMLRPCPort));
            DisposePlugins(true);
            _client.Connect();
        }

        private void Connected(object sender, EventArgs e)
        {
            Log.InfoToUI(string.Format("Connected to server {0} at port {1}.", ConfigSettings.ServerAddress, ConfigSettings.ServerXMLRPCPort));
        }

        #endregion

        #region Non Public Methods

        private void InitializePlugins()
        {
            Plugins = ConfigSettings.GetPlugins();
            HostContext = GetHostContext();

            if (HostContext == null)
            {
                Log.ErrorToUI("Could not create HostContext, stopping TMSPS!");
                return;
            }

            Log.InfoToUI(string.Format("{0} Plugins found, starting to initialize plugins.", Plugins.Count));

            foreach (ITMSPSPlugin plugin in Plugins)
            {
                plugin.InitPlugin(HostContext, new ConsoleUILogger("TMSPS", string.Format(" - [{0}]",plugin.ShortName)));
            }

            Log.InfoToUI("Plugins initialized.");
        }

        private void DisposePlugins(bool connectionLost)
        {
            foreach (ITMSPSPlugin plugin in Plugins)
            {
                plugin.DisposePlugin(connectionLost);
            }
        }

        private void ReadyForSendingCommands(object sender, EventArgs e)
        {
            try
            {
                GenericResponse<bool> authResponse = _client.Methods.Authenticate("SuperAdmin", ConfigSettings.SuperAdminPassword);

                if (authResponse == null || authResponse.Erroneous || !authResponse.Value)
                    Log.FatalToUI("Authentication failed!");
                else
                {
                    Log.InfoToUI("Authentication successfull");

                    GenericResponse<bool> enableCallbacksResponse = _client.Methods.EnableCallbacks(true);

                    if (enableCallbacksResponse.Erroneous || !enableCallbacksResponse.Value)
                    {
                        Log.ErrorToUI("Error enabling callbacks!");
                        Process.GetCurrentProcess().Close();
                        return;
                    }

                    InitializePlugins();
                }
            }
            catch (Exception ex)
            {
                Log.ErrorToUI("An exception was thrown during ReadyForSendingCommands stage!", ex);
                throw;
            }
        }

        private PluginHostContext GetHostContext()
        {
            GenericResponse<string> packMaskResponse = _client.Methods.GetServerPackMask();

            if (packMaskResponse.Erroneous)
            {
                Log.ErrorToUI("Error retrieving ServerPackMask: " + packMaskResponse.Fault.FaultMessage);
                return null;
            }

            GenericResponse<Version> versionResponse = _client.Methods.GetVersion();

            if (versionResponse.Erroneous)
            {
                Log.ErrorToUI("Error retrieving VersionInfo: " + packMaskResponse.Fault.FaultMessage);
                return null;
            }

            GenericResponse<string> directoryResponse = _client.Methods.GetTracksDirectory();

            if (directoryResponse.Erroneous)
            {
                Log.ErrorToUI("Error retrieving TracksDirectory: " + directoryResponse.Fault.FaultMessage);
                return null;
            }

            GenericResponse<DetailedPlayerInfo> serverPlayerInfo = _client.Methods.GetDetailedPlayerInfo(ConfigSettings.ServerLogin);

            if (serverPlayerInfo.Erroneous)
            {
                Log.ErrorToUI("Error retrieving server player details: " + serverPlayerInfo.Fault.FaultMessage);
                return null;
            }


            ServerInfo serverInfo = new ServerInfo(ConfigSettings, packMaskResponse.Value, versionResponse.Value, directoryResponse.Value, serverPlayerInfo.Value);
            MessageStyles messageStyles = MessageStyles.ReadFromFileOrGetDefault(Path.Combine(ApplicationDirectory, "MessageStyles.xml"));
            MessageConstants messageConstants = MessageConstants.ReadFromFile(Path.Combine(ApplicationDirectory, "MessageConstants.xml"), _client);

            return new PluginHostContext(_client, serverInfo, new Credentials(GetFullFilePath("Credentials.xml")), messageStyles, messageConstants, (TMSPSCorePlugin)Plugins[0]);
        }

        private void ReadConfigSettings()
        {
            Util.WaitUntilReadable(ApplicationConfigFileName, 10000);
            ConfigSettings = ConfigSettingsConfigurationSection.GetFromConfig("ConfigSettings");
        }

        private static string GetFullFilePath(string filename)
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return Path.Combine(directory, filename);
        }

        #endregion
    }
}