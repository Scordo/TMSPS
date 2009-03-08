using System;
using Version=TMSPS.Core.Communication.ProxyTypes.Version;

namespace TMSPS.Core.PluginSystem
{
    public class ServerInfo
    {
        public string ServerAddress { get; private set; }
        public int ServerXMLRpcPort { get; private set; }
        public string SuperAdminPassword { get; private set; }
        public string ServerNation { get; private set; }
        public string ServerLogin { get; private set; }
        public string ServerLoginPassword { get; private set; }
        public string ServerPackMask { get; private set; }
        public Version Version { get; private set; }
        public string TrackDirectory { get; private set; }

        public ServerInfo(ConfigSettingsConfigurationSection configSection, string serverPackMask, Version version, string trackDirectory)
        {
            if (configSection == null)
                throw new ArgumentNullException("configSection");

            if (serverPackMask == null)
                throw new ArgumentNullException("serverPackMask");

            if (version == null)
                throw new ArgumentNullException("version");

            if (trackDirectory == null)
                throw new ArgumentNullException("trackDirectory");

            ServerAddress = configSection.ServerAddress;
            ServerXMLRpcPort = configSection.ServerXMLRPCPort;
            SuperAdminPassword = configSection.SuperAdminPassword;
            ServerNation = configSection.ServerNation;
            ServerLogin = configSection.ServerLogin;
            ServerLoginPassword = configSection.ServerLoginPassword;
            ServerPackMask = serverPackMask;
            Version = version.Clone();
            TrackDirectory = trackDirectory;
        }
    }
}