using System.Configuration;
using System.IO;
using System.Xml.Linq;
using SettingsBase = TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class LocalRecordsUISettings : SettingsBase
    {
        #region Constants

        public const uint MAX_RECORDS_TO_SHOW = 20;
        public const string VOTE_ACCEPTED_MESSAGE = "{[#ServerStyle]}> {[#MessageStyle]}Vote accepted! Average vote is: {[#HighlightStyle]}{[AverageVote]}";
        public const string FIRST_LOCAL_RANK_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[Nickname]}$z{[#RecordStyle]} gained the {[#RankStyle]}{[Rank]}{[#RecordStyle]}. Local Record!";
        public const string NEW_LOCAL_RANK_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[Nickname]}$z{[#RecordStyle]} claimed the {[#RankStyle]}{[NewRank]}{[#RecordStyle]}. Local Record!";
        public const string IMPROVED_LOCAL_RANK_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[Nickname]}$z{[#RecordStyle]} secured his/her {[#RankStyle]}{[Rank]}{[#RecordStyle]}. Local Record!";
        public const string WIN_MESSAGE = "{[#ServerStyle]}> {[#RecordStyle]}Congratulations, you've won your {[#RankStyle]}{[Wins]}{[#RecordStyle]}. race!";
        public const string RANKING_MESSAGE = "{[#ServerStyle]}> {[#RecordStyle]}Your server rank is {[#HighlightStyle]}{[Rank]}{[#RecordStyle]}, Average: {[#HighlightStyle]}{[Average]}{[#RecordStyle]}, Score: {[#HighlightStyle]}{[Score]}{[#RecordStyle]}, Tracks {[#HighlightStyle]}{[Tracks]}{[#RecordStyle]}/{[#HighlightStyle]}{[TracksCount]}";
        public const string INFO_MESSAGE = "{[#ServerStyle]}> Your info: {[#MessageStyle]}Wins: {[#HighlightStyle]}{[Wins]}{[#MessageStyle]} Time played: {[#HighlightStyle]}{[Played]}{[#MessageStyle]} First visit: {[#HighlightStyle]}{[Created]}";
        public const bool SHOW_MESSAGES = true;
        public const uint NOTICE_DELAY_IN_SECONDS = 1;
        public const bool SHOW_LOCAL_RECORD_UI = true;
        public const bool SHOW_PB_RECORD_UI = true;
        public const bool SHOW_LOCAL_RECORD_LIST_UI = true;
        public const bool HIDE_RECORD_LIST_UI_ON_FINISH = true;
        public const bool STRIP_NICK_FORMATTING = false;
        public const uint UPDATE_INTERVAL = 2;

        public const double RECORDLIST_PLAYER_START_MARGIN = -2.9;
        public const double RECORDLIST_TOP3_GAP = 0.6;
        public const double RECORDLIST_PLAYER_RECORD_HEIGHT = 1.5;
        public const double RECORDLIST_PLAYER_END_MARGIN = 0;
        public const double RECORDLIST_PLAYER_TO_CONTAINER_MARGIN_Y = 0.7;


        #endregion

        #region Properties

        public uint MaxRecordsToShow { get; protected internal set; }
        public bool ShowMessages { get; private set; }
        public bool ShowPBUserInterface { get; private set; }
        public bool ShowLocalRecordUserInterface { get; private set; }
        public bool ShowLocalRecordListUserInterface { get; private set; }
        public bool HideRecordListUIOnFinish { get; private set; }
        public bool StripNickFormatting { get; private set; }
        public uint UpdateInterval { get; set; }

        public string VoteAcceptedMessage { get; private set; }
        public string FirstLocalRankMessage { get; private set; }
        public string NewLocalRankMessage { get; private set; }
        public string ImprovedLocalRankMessage { get; private set; }
        public string WinMessage { get; private set; }
        public string InfoMessage { get; private set; }
        public string RankingMessage { get; private set; }
        public uint NoticeDelayInSeconds { get; private set; }
        public string PBPanelTemplate { get; private set; }

        public string LocalRecordPanelTemplate { get; private set; }

        public double RecordListPlayerStartMargin { get; private set; }
        public double RecordListTop3Gap { get; private set; }
        public double RecordListPlayerRecordHeight { get; private set; }
        public double RecordListPlayerEndMargin { get; private set; }
        public double RecordListPlayerToContainerMarginY { get; private set; }

        public string RecordListMainTemplate { get; private set; }
        public string RecordListTop3RecordTemplate { get; private set; }
        public string RecordListRecordTemplate { get; private set; }
        public string RecordListRecordHighlightTemplate { get; private set; }

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
            result.ShowMessages = ReadConfigBool(configDocument.Root, "ShowMessages", SHOW_MESSAGES, xmlConfigurationFile);
            result.ShowPBUserInterface = ReadConfigBool(configDocument.Root, "ShowPBUI", SHOW_PB_RECORD_UI, xmlConfigurationFile);
            result.ShowLocalRecordUserInterface = ReadConfigBool(configDocument.Root, "ShowLRUI", SHOW_LOCAL_RECORD_UI, xmlConfigurationFile);
            result.ShowLocalRecordListUserInterface = ReadConfigBool(configDocument.Root, "ShowLRListUI", SHOW_LOCAL_RECORD_LIST_UI, xmlConfigurationFile);
            result.HideRecordListUIOnFinish = ReadConfigBool(configDocument.Root, "HideLRListUIOnFinish", HIDE_RECORD_LIST_UI_ON_FINISH, xmlConfigurationFile);
            result.VoteAcceptedMessage = ReadConfigString(configDocument.Root, "VoteAcceptedMessage", VOTE_ACCEPTED_MESSAGE, xmlConfigurationFile);
            result.FirstLocalRankMessage = ReadConfigString(configDocument.Root, "FirstLocalRankMessage", FIRST_LOCAL_RANK_MESSAGE, xmlConfigurationFile);
            result.NewLocalRankMessage = ReadConfigString(configDocument.Root, "NewLocalRankMessage", NEW_LOCAL_RANK_MESSAGE, xmlConfigurationFile);
            result.ImprovedLocalRankMessage = ReadConfigString(configDocument.Root, "ImprovedLocalRankMessage", IMPROVED_LOCAL_RANK_MESSAGE, xmlConfigurationFile);
            result.WinMessage = ReadConfigString(configDocument.Root, "WinMessage", WIN_MESSAGE, xmlConfigurationFile);
            result.InfoMessage = ReadConfigString(configDocument.Root, "InfoMessage", INFO_MESSAGE, xmlConfigurationFile);
            result.RankingMessage = ReadConfigString(configDocument.Root, "RankingMessage", RANKING_MESSAGE, xmlConfigurationFile);
            result.NoticeDelayInSeconds = ReadConfigUInt(configDocument.Root, "NoticeDelayInSeconds", NOTICE_DELAY_IN_SECONDS, xmlConfigurationFile);
            result.StripNickFormatting = ReadConfigBool(configDocument.Root, "StripNickFormatting", STRIP_NICK_FORMATTING, xmlConfigurationFile);
            result.UpdateInterval = ReadConfigUInt(configDocument.Root, "UpdateInterval", UPDATE_INTERVAL, xmlConfigurationFile);

            string pbPanelTemplateFile = Path.Combine(settingsDirectory, "PBPanelTemplate.xml");
            result.PBPanelTemplate = File.Exists(pbPanelTemplateFile) ? File.ReadAllText(pbPanelTemplateFile) : UITemplates.LowerRightPBRecordPanelActive;

            string localRecordPanelTemplateFile = Path.Combine(settingsDirectory, "LocalRecordPanelTemplate.xml");
            result.LocalRecordPanelTemplate = File.Exists(localRecordPanelTemplateFile) ? File.ReadAllText(localRecordPanelTemplateFile) : UITemplates.LowerRightLocalRecordPanelActive;


            string recordListTemplateFile = Path.Combine(settingsDirectory, "LocalRecordsListTemplate.xml");

            if (File.Exists(recordListTemplateFile))
            {
                XDocument listTemplateDocument = XDocument.Load(recordListTemplateFile);

                if (listTemplateDocument.Root == null)
                    throw new ConfigurationErrorsException("Could not find root node in file: " + listTemplateDocument);

                result.RecordListPlayerStartMargin = ReadConfigDouble(listTemplateDocument.Root, "PlayerStartMargin", result.RecordListPlayerStartMargin, recordListTemplateFile);
                result.RecordListTop3Gap = ReadConfigDouble(listTemplateDocument.Root, "Top3Gap", result.RecordListTop3Gap, recordListTemplateFile);
                result.RecordListPlayerRecordHeight = ReadConfigDouble(listTemplateDocument.Root, "PlayerRecordHeight", result.RecordListPlayerRecordHeight, recordListTemplateFile);
                result.RecordListPlayerEndMargin = ReadConfigDouble(listTemplateDocument.Root, "PlayerEndMargin", result.RecordListPlayerEndMargin, recordListTemplateFile);
                result.RecordListPlayerToContainerMarginY = ReadConfigDouble(listTemplateDocument.Root, "PlayerToContainerMarginY", result.RecordListPlayerToContainerMarginY, recordListTemplateFile);

                XElement templatesElement = listTemplateDocument.Root.Element("Templates");

                if (templatesElement != null)
                {
                    result.RecordListMainTemplate = ReadConfigString(templatesElement, "MainTemplate", result.RecordListMainTemplate, recordListTemplateFile);
                    result.RecordListTop3RecordTemplate = ReadConfigString(templatesElement, "Top3RecordTemplate", result.RecordListTop3RecordTemplate, recordListTemplateFile);
                    result.RecordListRecordTemplate = ReadConfigString(templatesElement, "RecordTemplate", result.RecordListRecordTemplate, recordListTemplateFile);
                    result.RecordListRecordHighlightTemplate = ReadConfigString(templatesElement, "RecordHighlightTemplate", result.RecordListRecordHighlightTemplate, recordListTemplateFile);
                }
            }

            return result;
        }

        #endregion
    }
}