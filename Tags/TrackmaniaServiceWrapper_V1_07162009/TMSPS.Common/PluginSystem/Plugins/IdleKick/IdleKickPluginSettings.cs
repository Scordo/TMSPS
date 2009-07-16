using System;
using System.Configuration;
using System.Xml.Linq;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.IdleKick
{
    public class IdleKickPluginSettings : SettingsBase
    {
        #region Constants

        public const IdleKickMode KICK_MODE = IdleKickMode.ROUNDS;
        public const uint ROUNDS_COUNT = 5;
        public const uint SECONDS_COUNT = 1500;
        public const bool KICK_SPECTATORS = true;
        public const bool RESET_ON_FINISH = true;
        public const bool RESET_ON_CHAT = true;
        public const bool RESET_ON_CHECKPOINT = true;
        public const string PUBLIC_KICK_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[Nickname]} {[#MessageStyle]} got kicked for idling too long.";
        public const string PRIVATE_KICK_MESSAGE = "You were kicked for idling too long.";

        #endregion

        #region Properties

        public IdleKickMode KickMode { get; protected set; }
        public uint RoundsCount { get; protected set; }
        public uint SecondsCount { get; protected set; }
        public bool KickSpectators { get; protected set; }
        public bool ResetOnFinish { get; protected set; }
        public bool ResetOnChat { get; protected set; }
        public bool ResetOnCheckpoint { get; protected set; }
        public string PublicKickMessage { get; protected set; }
        public string PrivateKickMessage { get; protected set; }


        #endregion

        #region Public Methods

        public static IdleKickPluginSettings ReadFromFile(string xmlConfigurationFile)
        {
            IdleKickPluginSettings result = new IdleKickPluginSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            string kickModeString = ReadConfigString(configDocument.Root, "KickMode", "Rounds", xmlConfigurationFile).ToUpper();
            result.KickMode = IdleKickMode.ROUNDS;

            if (Enum.IsDefined(typeof(IdleKickMode), kickModeString))
                result.KickMode = (IdleKickMode) Enum.Parse(typeof (IdleKickMode), kickModeString);

            result.KickSpectators = ReadConfigBool(configDocument.Root, "KickSpectators", KICK_SPECTATORS, xmlConfigurationFile);
            result.RoundsCount = ReadConfigUInt(configDocument.Root, "IdleRoundsCount", ROUNDS_COUNT, xmlConfigurationFile);
            result.SecondsCount = ReadConfigUInt(configDocument.Root, "IdleSecondsCount", SECONDS_COUNT, xmlConfigurationFile);
            result.ResetOnFinish = ReadConfigBool(configDocument.Root, "ResetOnFinish", RESET_ON_FINISH, xmlConfigurationFile);
            result.ResetOnChat = ReadConfigBool(configDocument.Root, "ResetOnChat", RESET_ON_CHAT, xmlConfigurationFile);
            result.ResetOnCheckpoint = ReadConfigBool(configDocument.Root, "ResetOnCheckpoint", RESET_ON_CHECKPOINT, xmlConfigurationFile);
            result.PublicKickMessage = ReadConfigString(configDocument.Root, "PublicKickMessage", PUBLIC_KICK_MESSAGE, xmlConfigurationFile);
            result.PrivateKickMessage = ReadConfigString(configDocument.Root, "PrivateKickMessage", PRIVATE_KICK_MESSAGE, xmlConfigurationFile);
           
            return result;
        }

        #endregion
    }
}
