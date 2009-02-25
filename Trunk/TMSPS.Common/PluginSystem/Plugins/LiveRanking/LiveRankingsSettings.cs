using System.Configuration;
using System.Xml.Linq;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.LiveRanking
{
    public class LiveRankingsSettings : SettingsBase
    {
        #region Constants

        public const uint MAX_RECORDS_TO_SHOW = 20;
        public const uint UPDATE_INTERVAL_IN_SECONDS = 2;

        public const double RANKING_PLAYER_START_MARGIN = -2.9;
        public const double RANKING_TOP3_GAP = 0.6;
        public const double RANKING_PLAYER_RECORD_HEIGHT = 1.5;
        public const double RANKING_PLAYER_END_MARGIN = 0;
        public const double RANKING_PLAYER_TO_CONTAINER_MARGIN_Y = 0.7;

        #endregion

        #region Properties

        public uint MaxRecordsToShow { get; protected internal set; }
        public uint UpdateInterval { get; private set; }

        public double RankingPlayerStartMargin { get; private set; }
        public double RankingTop3Gap { get; private set; }
        public double RankingPlayerRecordHeight { get; private set; }
        public double RankingPlayerEndMargin { get; private set; }
        public double RankingPlayerToContainerMarginY { get; private set; }

        public string RankingListTemplate { get; private set; }
        public string RankingTop3RecordTemplate { get; private set; }
        public string RankingTemplate { get; private set; }
        public string RankingHighlightTemplate { get; private set; }

        #endregion

        #region Public Methods

        public static LiveRankingsSettings ReadFromFile(string xmlConfigurationFile)
        {
            LiveRankingsSettings result = new LiveRankingsSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.MaxRecordsToShow = ReadConfigUInt(configDocument.Root, "MaxRecordsToShow", MAX_RECORDS_TO_SHOW, xmlConfigurationFile);
            result.UpdateInterval = ReadConfigUInt(configDocument.Root, "UpdateIntervalInSeconds", UPDATE_INTERVAL_IN_SECONDS, xmlConfigurationFile);

            result.RankingPlayerToContainerMarginY = RANKING_PLAYER_TO_CONTAINER_MARGIN_Y;
            result.RankingPlayerStartMargin = RANKING_PLAYER_START_MARGIN;
            result.RankingPlayerEndMargin = RANKING_PLAYER_END_MARGIN;
            result.RankingPlayerRecordHeight = RANKING_PLAYER_RECORD_HEIGHT;
            result.RankingTop3Gap = RANKING_TOP3_GAP;

            result.RankingListTemplate = UITemplates.RankingListTemplate;
            result.RankingTop3RecordTemplate = UITemplates.RankingTop3RecordTemplate;
            result.RankingTemplate = UITemplates.RankingTemplate;
            result.RankingHighlightTemplate = UITemplates.RankingHighlightTemplate;

            return result;
        }

        #endregion
    }
}