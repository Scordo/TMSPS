using System.Xml.Linq;
using System.Configuration;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using SettingsBase=TMSPS.Core.Common.SettingsBase;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins
{
    public class KickUnrankedPlayerPlugin : TMSPSPlugin
    {
        #region Properties

        public override Version Version
        {
            get { return new Version("1.0.0.0"); }
        }

        public override string Author
        {
            get { return "Jens Hofmann"; }
        }

        public override string Name
        {
            get { return "ChatBotPlugin"; }
        }

        public override string Description
        {
            get { return "Checks for players having no rank. And automatically kicks them"; }
        }

        public override string ShortName
        {
            get { return "KickUnrankedPlayers"; }
        }

        public KickUnrankedPlayerPluginSettings Settings { get; private set; }

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = KickUnrankedPlayerPluginSettings.ReadFromFile(PluginSettingsFilePath);
            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
        }

        private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
        {
            DetailedPlayerInfo detailedPlayerInfo = GetDetailedPlayerInfo(e.Login);

            if (detailedPlayerInfo == null)
                return;

            PlayerRanking worldRanking = detailedPlayerInfo.LadderStats.PlayerRankings.Find(ranking => ranking.Path == "World");

            if (worldRanking == null)
                return;

            if (worldRanking.Ranking == -1)
            {
                Context.RPCClient.Methods.Kick(e.Login, Settings.PersonalKickMessage);
                Context.RPCClient.Methods.ChatSend(Settings.PublicKickMessage.Replace("{{Nickname]}", detailedPlayerInfo.NickName));
            }
        }

        protected override void Dispose(bool connectionLost)
        {
            
        }

        public DetailedPlayerInfo GetDetailedPlayerInfo(string login)
        {
            GenericResponse<DetailedPlayerInfo> playerInfoResponse = Context.RPCClient.Methods.GetDetailedPlayerInfo(login);

            if (playerInfoResponse.Erroneous)
            {
                Logger.Error(string.Format("Error getting detailed Playerinfo for player with login {0}: {1}", login, playerInfoResponse.Fault.FaultMessage));
                Logger.ErrorToUI(string.Format("Error getting detailed Playerinfo for player with login {0}", login));
                return null;
            }

            return playerInfoResponse.Value;
        }

        #endregion
    }

    public class KickUnrankedPlayerPluginSettings : SettingsBase
    {
        public const string PERSONAL_KICK_MESSAGE = "Unranked players are not welcome at this server.";
        public const string PUBLIC_KICK_MESSAGE = "{{Nickname]} got kicked for not having a ranking.";

        public string PersonalKickMessage { get; private set; }
        public string PublicKickMessage { get; private set; }

        public static KickUnrankedPlayerPluginSettings ReadFromFile(string xmlConfigurationFile)
        {
            //string settingsDirectory = Path.GetDirectoryName(xmlConfigurationFile);

            KickUnrankedPlayerPluginSettings result = new KickUnrankedPlayerPluginSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.PublicKickMessage = ReadConfigString(configDocument.Root, "PublicKickMessage", PUBLIC_KICK_MESSAGE, xmlConfigurationFile);
            result.PersonalKickMessage = ReadConfigString(configDocument.Root, "PersonalKickMessage", PERSONAL_KICK_MESSAGE, xmlConfigurationFile);
            

            return result;
        }
    }
}
