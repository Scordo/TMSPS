using System.Xml.Linq;
using System.Configuration;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
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
                SendFormattedMessage(Settings.PublicKickMessage, "Nickname", StripTMColorsAndFormatting(detailedPlayerInfo.NickName));
                e.Handled = true;
            }
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
        }

        #endregion
    }

    public class KickUnrankedPlayerPluginSettings : SettingsBase
    {
        #region Constants

        public const string PERSONAL_KICK_MESSAGE = "Unranked players are not welcome at this server.";
        public const string PUBLIC_KICK_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[Nickname]} {[#MessageStyle]} got kicked for not having a ranking.";

        #endregion

        #region Properties

        public string PersonalKickMessage { get; private set; }
        public string PublicKickMessage { get; private set; }

        #endregion

        #region Public Methods

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

        #endregion
    }
}