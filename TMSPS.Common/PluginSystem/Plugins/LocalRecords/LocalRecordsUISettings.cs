using System.Configuration;
using System.Xml.Linq;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class LocalRecordsUISettings : SettingsBase
    {
        #region Constants

        public const uint MAX_RECORDS_TO_SHOW = 20;
        public const string VOTE_ACCEPTED_MESSAGE = "$zVote accepted! Average vote is: {[AverageVote]}";
        public const string FIRST_LOCAL_RANK_MESSAGE = "$z{[Nickname]}$z got his/her first local rank: $w$s$0f0{[Rank]}$z!";
        public const string NEW_LOCAL_RANK_MESSAGE = "$z{[Nickname]}$z achieved local rank: $w$s$0f0{[NewRank]}$z. Old rank: $w$s{[OldRank]}";
        public const string IMPROVED_LOCAL_RANK_MESSAGE = "$z{[Nickname]}$z improved his/her local rank: $w$s$0f0{[Rank]}$z!";
        public const bool SHOW_MESSAGES_DEFAULT = true;
        public const uint NOTICE_DELAY_IN_SECONDS = 1;

        #endregion

        #region Properties

        public uint MaxRecordsToShow { get; protected internal set; }
        public bool ShowMessages { get; private set; }
        public string VoteAcceptedMessage { get; private set; }
        public string FirstLocalRankMessage { get; private set; }
        public string NewLocalRankMessage { get; private set; }
        public string ImprovedLocalRankMessage { get; private set; }
        public uint NoticeDelayInSeconds { get; private set; }

        #endregion

        #region Methods

        public static LocalRecordsUISettings ReadFromFile(string xmlConfigurationFile)
        {
            LocalRecordsUISettings result = new LocalRecordsUISettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.MaxRecordsToShow = ReadConfigUInt(configDocument.Root, "MaxRecordsToShow", MAX_RECORDS_TO_SHOW, xmlConfigurationFile);
            result.ShowMessages = ReadConfigBool(configDocument.Root, "ShowMessages", SHOW_MESSAGES_DEFAULT, xmlConfigurationFile);
            result.VoteAcceptedMessage = ReadConfigString(configDocument.Root, "VoteAcceptedMessage", VOTE_ACCEPTED_MESSAGE, xmlConfigurationFile);
            result.FirstLocalRankMessage = ReadConfigString(configDocument.Root, "FirstLocalRankMessage", FIRST_LOCAL_RANK_MESSAGE, xmlConfigurationFile);
            result.NewLocalRankMessage = ReadConfigString(configDocument.Root, "NewLocalRankMessage", NEW_LOCAL_RANK_MESSAGE, xmlConfigurationFile);
            result.ImprovedLocalRankMessage = ReadConfigString(configDocument.Root, "ImprovedLocalRankMessage", IMPROVED_LOCAL_RANK_MESSAGE, xmlConfigurationFile);
            result.NoticeDelayInSeconds = ReadConfigUInt(configDocument.Root, "NoticeDelayInSeconds", NOTICE_DELAY_IN_SECONDS, xmlConfigurationFile);

            return result;
        }

        #endregion
    }
}