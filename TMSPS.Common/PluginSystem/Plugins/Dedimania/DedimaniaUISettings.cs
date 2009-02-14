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

        public const string NEW_DEDIMANIA_RANK_MESSAGE = "$z{[Nickname]}$z claimed dedimania rank: $w$s$0f0{[Rank]}$z!";
        public const string IMPROVED_DEDIMANIA_RANK_MESSAGE = "$z{[Nickname]}$z improved his/her dedimania rank: $w$s$0f0{[Rank]}$z!";
        public const bool SHOW_DEDIMANIA_RECORD_UI = true;

        #endregion

        #region Properties

        public uint MaxRecordsToShow { get; protected internal set; }
        public bool ShowMessages { get; private set; }
        public bool ShowDedimaniaRecordUI { get; private set; }
        public uint NoticeDelayInSeconds { get; private set; }
        public string DediPanelTemplateActive { get; private set; }
        public string DediPanelTemplateInactive { get; private set; }
        public string NewDedimaniaRankMessage { get; private set; }
        public string ImprovedDedimaniaRankMessage { get; private set; }

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
            result.ShowDedimaniaRecordUI = ReadConfigBool(configDocument.Root, "ShowDEDUI", SHOW_DEDIMANIA_RECORD_UI, xmlConfigurationFile);
            result.NoticeDelayInSeconds = ReadConfigUInt(configDocument.Root, "NoticeDelayInSeconds", NOTICE_DELAY_IN_SECONDS, xmlConfigurationFile);
            result.NewDedimaniaRankMessage = ReadConfigString(configDocument.Root, "NewDedimaniaRankMessage", NEW_DEDIMANIA_RANK_MESSAGE, xmlConfigurationFile);
            result.ImprovedDedimaniaRankMessage = ReadConfigString(configDocument.Root, "ImprovedDedimaniaRankMessage", IMPROVED_DEDIMANIA_RANK_MESSAGE, xmlConfigurationFile);

            string dediPanelActiveTemplateFile = Path.Combine(settingsDirectory, "DediPanelActiveTemplate.xml");
            result.DediPanelTemplateActive = File.Exists(dediPanelActiveTemplateFile) ? File.ReadAllText(dediPanelActiveTemplateFile) : UITemplates.LowerRightDediRecordPanelActive;
            string dediPanelInactiveTemplateFile = Path.Combine(settingsDirectory, "DediPanelInactiveTemplate.xml");
            result.DediPanelTemplateInactive = File.Exists(dediPanelInactiveTemplateFile) ? File.ReadAllText(dediPanelInactiveTemplateFile) : UITemplates.LowerRightDediRecordPanelInActive;
            
            return result;
        }

        #endregion

    }
}
