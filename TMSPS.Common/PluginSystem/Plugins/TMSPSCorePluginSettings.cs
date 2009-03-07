using System.Configuration;
using System.Xml.Linq;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins
{
    public class TMSPSCorePluginSettings : SettingsBase
    {
        #region Constants

        public const string JOIN_MESSAGE = "{[#ServerStyle]}>> {[#MessageStyle]}New Player: {[#HighlightStyle]}{[Nickname]}{[#MessageStyle]} Nation: {[#HighlightStyle]}{[Nation]}{[#MessageStyle]} Ladder: {[#HighlightStyle]}{[Ladder]}";
        public const bool ENABLE_JOIN_MESSAGE = true;
        public const string LEAVE_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[Nickname]}{[#MessageStyle]} has left the game.";
        public const bool ENABLE_LEAVE_MESSAGE = true;
        public const string KICK_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[KickingNickname]}{[#MessageStyle]} kicked {[#HighlightStyle]}{[KickedNickname]}.";
        public const string BAN_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[BanningNickname]}{[#MessageStyle]} banned {[#HighlightStyle]}{[BannedNickname]}.";
        public const string BLACKLIST_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[BlackListingNickname]}{[#MessageStyle]} blacklists {[#HighlightStyle]}{[BlackListedNickname]}.";

        #endregion

        #region Properties

        public bool EnableJoinMessage { get; private set; }
        public string JoinMessage { get; private set; }
        public string LeaveMessage { get; private set; }
        public bool EnableLeaveMessage { get; private set; }
        public string KickMessage { get; private set; }
        public string BanMessage { get; private set; }
        public string BlackListMessage { get; private set; }

        #endregion

        #region Public Methods

        public static TMSPSCorePluginSettings ReadFromFile(string xmlConfigurationFile)
        {
            //string settingsDirectory = Path.GetDirectoryName(xmlConfigurationFile);

            TMSPSCorePluginSettings result = new TMSPSCorePluginSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.EnableJoinMessage = ReadConfigBool(configDocument.Root, "EnableJoinMessage", ENABLE_JOIN_MESSAGE, xmlConfigurationFile);
            result.JoinMessage = ReadConfigString(configDocument.Root, "JoinMessage", JOIN_MESSAGE, xmlConfigurationFile);
            result.EnableLeaveMessage = ReadConfigBool(configDocument.Root, "EnableLeaveMessage", ENABLE_LEAVE_MESSAGE, xmlConfigurationFile);
            result.LeaveMessage = ReadConfigString(configDocument.Root, "LeaveMessage", LEAVE_MESSAGE, xmlConfigurationFile);
            result.KickMessage = ReadConfigString(configDocument.Root, "KickMessage", KICK_MESSAGE, xmlConfigurationFile);
            result.BanMessage = ReadConfigString(configDocument.Root, "BanMessage", BAN_MESSAGE, xmlConfigurationFile);
            result.BlackListMessage = ReadConfigString(configDocument.Root, "BlackListMessage", BLACKLIST_MESSAGE, xmlConfigurationFile);
            

            return result;
        }

        #endregion
    }
}