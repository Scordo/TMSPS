using System.Configuration;
using System.IO;
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
        public const bool SHOW_LOCAL_RECORD_UI_DEFAULT = true;
        public const bool SHOW_PB_RECORD_UI_DEFAULT = true;

        #endregion

        #region Properties

        public uint MaxRecordsToShow { get; protected internal set; }
        public bool ShowMessages { get; private set; }
        public bool ShowPBUserInterface { get; private set; }
        public bool ShowLocalRecordUserInterface { get; private set; }

        public string VoteAcceptedMessage { get; private set; }
        public string FirstLocalRankMessage { get; private set; }
        public string NewLocalRankMessage { get; private set; }
        public string ImprovedLocalRankMessage { get; private set; }
        public uint NoticeDelayInSeconds { get; private set; }
        public string PBPanelTemplateActive { get; private set; }
        public string PBPanelTemplateInactive { get; private set; }

        public string LocalRecordPanelTemplateActive { get; private set; }
        public string LocalRecordPanelTemplateInactive { get; private set; }

        #endregion

        #region Methods

        public static LocalRecordsUISettings ReadFromFile(string xmlConfigurationFile)
        {
            string settingsDirectory = Path.GetDirectoryName(xmlConfigurationFile);

            LocalRecordsUISettings result = new LocalRecordsUISettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.MaxRecordsToShow = ReadConfigUInt(configDocument.Root, "MaxRecordsToShow", MAX_RECORDS_TO_SHOW, xmlConfigurationFile);
            result.ShowMessages = ReadConfigBool(configDocument.Root, "ShowMessages", SHOW_MESSAGES_DEFAULT, xmlConfigurationFile);
            result.ShowPBUserInterface = ReadConfigBool(configDocument.Root, "ShowPBUI", SHOW_PB_RECORD_UI_DEFAULT, xmlConfigurationFile);
            result.ShowLocalRecordUserInterface = ReadConfigBool(configDocument.Root, "ShowLRUI", SHOW_LOCAL_RECORD_UI_DEFAULT, xmlConfigurationFile);
            result.VoteAcceptedMessage = ReadConfigString(configDocument.Root, "VoteAcceptedMessage", VOTE_ACCEPTED_MESSAGE, xmlConfigurationFile);
            result.FirstLocalRankMessage = ReadConfigString(configDocument.Root, "FirstLocalRankMessage", FIRST_LOCAL_RANK_MESSAGE, xmlConfigurationFile);
            result.NewLocalRankMessage = ReadConfigString(configDocument.Root, "NewLocalRankMessage", NEW_LOCAL_RANK_MESSAGE, xmlConfigurationFile);
            result.ImprovedLocalRankMessage = ReadConfigString(configDocument.Root, "ImprovedLocalRankMessage", IMPROVED_LOCAL_RANK_MESSAGE, xmlConfigurationFile);
            result.NoticeDelayInSeconds = ReadConfigUInt(configDocument.Root, "NoticeDelayInSeconds", NOTICE_DELAY_IN_SECONDS, xmlConfigurationFile);

            string pbPanelActiveTemplateFile = Path.Combine(settingsDirectory, "PBPanelActiveTemplate.xml");
            result.PBPanelTemplateActive = File.Exists(pbPanelActiveTemplateFile) ? File.ReadAllText(pbPanelActiveTemplateFile) : UITemplates.LowerRightPBPanelActive;
            string pbPanelInactiveTemplateFile = Path.Combine(settingsDirectory, "PBPanelInactiveTemplate.xml");
            result.PBPanelTemplateInactive = File.Exists(pbPanelInactiveTemplateFile) ? File.ReadAllText(pbPanelInactiveTemplateFile) : UITemplates.LowerRightPBPanelInactive;

            string localRecordPanelActiveTemplateFile = Path.Combine(settingsDirectory, "LocalRecordPanelActiveTemplate.xml");
            result.LocalRecordPanelTemplateActive = File.Exists(localRecordPanelActiveTemplateFile) ? File.ReadAllText(localRecordPanelActiveTemplateFile) : UITemplates.LowerRightLocalRecordPanelActive;
            string localRecordPanelInactiveTemplateFile = Path.Combine(settingsDirectory, "LocalRecordPanelInactiveTemplate.xml");
            result.LocalRecordPanelTemplateInactive = File.Exists(localRecordPanelInactiveTemplateFile) ? File.ReadAllText(localRecordPanelInactiveTemplateFile) : UITemplates.LowerRightLocalRecordPanelInactive;

            return result;
        }

        #endregion
    }
}