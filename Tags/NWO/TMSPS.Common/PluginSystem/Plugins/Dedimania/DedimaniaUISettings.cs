using System.IO;
using System.Xml.Linq;
using System.Configuration;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania
{
    public class DedimaniaUISettings : SettingsBase
    {
        #region Constants

        public const uint MAX_RECORDS_TO_SHOW = 20;
        public const bool SHOW_MESSAGES = true;
        public const uint NOTICE_DELAY_IN_SECONDS = 1;

        public const string NEW_RANK_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[Nickname]}$z{[#DedimaniaRecordStyle]} claimed the {[#RankStyle]}{[Rank]}{[#DedimaniaRecordStyle]}. Dedimania Record!";
        public const string IMPROVED_RANK_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[Nickname]}$z{[#DedimaniaRecordStyle]} improved his/her {[#RankStyle]}{[Rank]}{[#DedimaniaRecordStyle]}. Dedimania Record!";
        public const bool SHOW_RECORD_UI = true;
        public const bool SHOW_RECORD_LIST_UI = true;
        public const bool HIDE_RECORD_LIST_UI_ON_FINISH = true;
        public const bool STRIP_NICK_FORMATTING = false;
        public const uint STATIC_MODE_START_LIMIT = 40;

        public const double RECORDLIST_PLAYER_START_MARGIN = -2.9;
        public const double RECORDLIST_TOP3_GAP = 0.6;
        public const double RECORDLIST_PLAYER_RECORD_HEIGHT = 1.5;
        public const double RECORDLIST_PLAYER_END_MARGIN = 0;
        public const double RECORDLIST_PLAYER_TO_CONTAINER_MARGIN_Y = 0.7;
        

        #endregion

        #region Properties

        public uint MaxRecordsToShow { get; protected internal set; }
        public bool ShowMessages { get; private set; }
        public bool ShowRecordUI { get; private set; }
        public bool ShowRecordListUI { get; private set; }
        public bool HideRecordListUIOnFinish { get; private set; }
        public uint NoticeDelayInSeconds { get; private set; }
        public string DediPanelTemplateActive { get; private set; }
        public string NewRankMessage { get; private set; }
        public string ImprovedRankMessage { get; private set; }
        public bool StripNickFormatting { get; private set; }
        public uint StaticModeStartLimit { get; private set; }

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


        #region Public Methods

        public static DedimaniaUISettings ReadFromFile(string xmlConfigurationFile)
        {
            string settingsDirectory = Path.GetDirectoryName(xmlConfigurationFile);

            DedimaniaUISettings result = new DedimaniaUISettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.MaxRecordsToShow = ReadConfigUInt(configDocument.Root, "MaxRecordsToShow", MAX_RECORDS_TO_SHOW, xmlConfigurationFile);
            result.ShowMessages = ReadConfigBool(configDocument.Root, "ShowMessages", SHOW_MESSAGES, xmlConfigurationFile);
            result.ShowRecordUI = ReadConfigBool(configDocument.Root, "ShowDEDUI", SHOW_RECORD_UI, xmlConfigurationFile);
            result.ShowRecordListUI = ReadConfigBool(configDocument.Root, "ShowDEDListUI", SHOW_RECORD_LIST_UI, xmlConfigurationFile);
            result.HideRecordListUIOnFinish = ReadConfigBool(configDocument.Root, "HideDEDListUIOnFinish", HIDE_RECORD_LIST_UI_ON_FINISH, xmlConfigurationFile);
            result.NoticeDelayInSeconds = ReadConfigUInt(configDocument.Root, "NoticeDelayInSeconds", NOTICE_DELAY_IN_SECONDS, xmlConfigurationFile);
            result.NewRankMessage = ReadConfigString(configDocument.Root, "NewDedimaniaRankMessage", NEW_RANK_MESSAGE, xmlConfigurationFile);
            result.ImprovedRankMessage = ReadConfigString(configDocument.Root, "ImprovedDedimaniaRankMessage", IMPROVED_RANK_MESSAGE, xmlConfigurationFile);
            result.StripNickFormatting = ReadConfigBool(configDocument.Root, "StripNickFormatting", STRIP_NICK_FORMATTING, xmlConfigurationFile);
            result.StaticModeStartLimit = ReadConfigUInt(configDocument.Root, "StaticModeStartLimit", STATIC_MODE_START_LIMIT, xmlConfigurationFile);

            string dediPanelActiveTemplateFile = Path.Combine(settingsDirectory, "DediPanelTemplate.xml");
            result.DediPanelTemplateActive = File.Exists(dediPanelActiveTemplateFile) ? File.ReadAllText(dediPanelActiveTemplateFile) : UITemplates.LowerRightDediRecordPanel;

            result.RecordListPlayerToContainerMarginY = RECORDLIST_PLAYER_TO_CONTAINER_MARGIN_Y;
            result.RecordListPlayerStartMargin = RECORDLIST_PLAYER_START_MARGIN;
            result.RecordListPlayerEndMargin = RECORDLIST_PLAYER_END_MARGIN;
            result.RecordListPlayerRecordHeight = RECORDLIST_PLAYER_RECORD_HEIGHT;
            result.RecordListTop3Gap = RECORDLIST_TOP3_GAP;

            result.RecordListMainTemplate = UITemplates.RecordListMainTemplate;
            result.RecordListTop3RecordTemplate = UITemplates.RecordListTop3RecordTemplate;
            result.RecordListRecordTemplate = UITemplates.RecordListRecordTemplate;
            result.RecordListRecordHighlightTemplate = UITemplates.RecordListRecordHighlightTemplate;

            string recordListTemplateFile = Path.Combine(settingsDirectory, "DedimaniaRecordListTemplate.xml");

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
