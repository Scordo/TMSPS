using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.PluginSystem.Configuration;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.LiveRanking
{
    public class LiveRankingPlugin : TMSPSPlugin
    {
        #region Constants

        private const string LIVE_RANKING_LIST_MANIA_LINK_PAGE_ID = "LiveRankingListPanelID";

        #endregion

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
            get { return "Live Ranking Plugin"; }
        }

        public override string Description
        {
            get { return "Displays live rankings in a userinterface."; }
        }

        public override string ShortName
        {
            get { return "LiveRanking"; }
        }

        private LiveRankingsSettings Settings { get; set; }
        private PlayerRank[] LastRankings { get; set; }
        private TimedVolatileExecutionQueue<LiveRankingPlugin> UpdateTimer { get; set; }
        private bool PodiumStage { get; set; }

        #endregion

        #region Methods

        protected override void Init()
        {
            PodiumStage = false; 
            Settings = LiveRankingsSettings.ReadFromFile(PluginSettingsFilePath);
            LastRankings = new PlayerRank[]{};
            UpdateUI(this);
            UpdateTimer = new TimedVolatileExecutionQueue<LiveRankingPlugin>(TimeSpan.FromSeconds(Settings.UpdateInterval));

            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerFinish += Callbacks_PlayerFinish;
        }

        private void Callbacks_PlayerFinish(object sender, Communication.EventArguments.Callbacks.PlayerFinishEventArgs e)
        {
            if (e.TimeOrScore <= 0)
                return;

            UpdateTimer.Enqueue(UpdateUI, this);
        }

        private void Callbacks_PlayerConnect(object sender, Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            if (e.Handled)
                return;

            RunCatchLog(() => SendUIToPlayer(LastRankings, GetPlayerSettings(e.Login)), "Error in Callbacks_PlayerConnect Method.", true);
        }

        private void Callbacks_BeginRace(object sender, Communication.EventArguments.Callbacks.BeginRaceEventArgs e)
        {
            PodiumStage = false;
            RunCatchLog(() => 
            {
                UpdateTimer.Clear();
                UpdateUI(this);
            }, "Error in Callbacks_BeginRace Method.", true);
        }

        private void Callbacks_EndRace(object sender, Communication.EventArguments.Callbacks.EndRaceEventArgs e)
        {
            PodiumStage = true;
            
            RunCatchLog(() =>
            {
                UpdateTimer.Clear();
                UpdateUI(this);
            }, "Error in Callbacks_EndRace Method.", true);
        }

        private void UpdateUI(object state)
        {
            RunCatchLog(() =>
            {
                if (PodiumStage)
                {
                    HideUI();
                    return;
                }

                List<PlayerRank> rankings = GetCurrentRanking();
                if (rankings == null)
                    return;

                rankings.RemoveAll(rank => rank.BestTime <= 0);
                rankings.Sort((x,y) => x.Rank - y.Rank);
                PlayerRank[] rankingArray = rankings.ToArray();
                LastRankings = rankingArray;


				if (PlayersCount < Settings.StaticModeStartLimit)
				{
					foreach (PlayerSettings playerSettings in Context.PlayerSettings.GetAllAsList())
					{
						SendUIToPlayer(rankingArray, playerSettings);
					}
				}
				else
				{
					Context.RPCClient.Methods.SendDisplayManialinkPage(GetRecordListManiaLinkPage(rankingArray, null), 0, false);
				}
            }, "Error in UpdateUI Method.", true);
        }

        private void SendUIToPlayer(PlayerRank[] rankings, PlayerSettings playerSettings)
        {
            if (playerSettings == null)
                return;

            string maniaLinkPageContent = GetRecordListManiaLinkPage(rankings, PlayersCount < Settings.StaticModeStartLimit ? playerSettings.Login : null);
            string hash = maniaLinkPageContent.ToHash();

            if (playerSettings.ManiaLinkPageHashStore.Get(LIVE_RANKING_LIST_MANIA_LINK_PAGE_ID) == hash)
                return;

            playerSettings.ManiaLinkPageHashStore[LIVE_RANKING_LIST_MANIA_LINK_PAGE_ID] = hash; 
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(playerSettings.Login, maniaLinkPageContent, 0, false);
        }

        private string GetRecordListManiaLinkPage(PlayerRank[] rankings, string login)
        {
            // show at least top 3 
            int recordsToShow = Convert.ToInt32(Math.Max(Math.Min(Settings.MaxRecordsToShow, rankings.Length), 3));

            double totalHeight = Math.Abs(Settings.RankingPlayerToContainerMarginY * 2) + Math.Abs(Settings.RankingPlayerRecordHeight * recordsToShow) + Math.Abs(Settings.RankingTop3Gap) + Math.Abs(Settings.RankingPlayerEndMargin);

            XElement mainTemplate = XElement.Parse(Settings.RankingListTemplate.Replace("{[ManiaLinkID]}", LIVE_RANKING_LIST_MANIA_LINK_PAGE_ID).Replace("{[ContainerHeight}}", totalHeight.ToString(Context.Culture)));
            XElement rankingPlaceHolder = mainTemplate.Descendants("RankingPlaceHolder").First();
            double currentY = Settings.RankingPlayerStartMargin;

            XElement lastInsertedNode = rankingPlaceHolder;

            SortedList<uint, PlayerRank> localRankings = GetRankingsToShow(rankings, login, Settings.MaxRecordsToShow);

            foreach (KeyValuePair<uint, PlayerRank> localRanking in localRankings)
            {
                XElement currentElement = GetPlayerRecordElement(localRanking.Value, currentY, localRanking.Key, login);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= Settings.RankingPlayerRecordHeight;

                if (localRanking.Key == 3 && localRankings.Count > 3)
                    currentY -= Settings.RankingTop3Gap;
            }

            rankingPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetPlayerRecordElement(PlayerRank rankingInfo, double currentY, uint currentRank, string login)
        {
            string templateXML = Settings.RankingTemplate;

            if (rankingInfo.Login == login)
                templateXML = Settings.RankingHighlightTemplate;
            else if (currentRank <= 3)
                templateXML = Settings.RankingTop3RecordTemplate;

            TimeSpan time = TimeSpan.FromMilliseconds(rankingInfo.BestTime);

            StringBuilder playerRecordXml = new StringBuilder(templateXML);
            playerRecordXml.Replace("{[Y]}", currentY.ToString(CultureInfo.InvariantCulture));
            playerRecordXml.Replace("{[Rank]}", currentRank + ".");
            playerRecordXml.Replace("{[TimeOrScore]}", rankingInfo.BestTime <= 0 ? "  --.--  " : string.Format("{0}:{1}.{2}", time.Minutes, time.Seconds.ToString("00"), (time.Milliseconds / 10).ToString("00")));
            string nickname = SecurityElement.Escape(rankingInfo.NickName);

            if (Settings.StripNickFormatting)
                nickname = StripTMColorsAndFormatting(nickname);

            playerRecordXml.Replace("{[Nickname]}", nickname);

            return XElement.Parse(playerRecordXml.ToString());
        }

        public static SortedList<uint, PlayerRank> GetRankingsToShow(PlayerRank[] rankings, string login, uint maxRecordsToShow)
        {
            // show at least the top 3
            maxRecordsToShow = (uint)Math.Max(Math.Min(maxRecordsToShow, rankings.Length), 3);
            uint maxRecordsToReport = (uint)rankings.Length;

            // set maxRecordsToShow to amount of existing rankings when the amoutn of rankings is less than the value of maxRecordsToShow
            if (rankings.Length < maxRecordsToShow && rankings.Length > 3)
                maxRecordsToShow = Convert.ToUInt32(rankings.Length);

            int currentPlayerRankIndex = Array.FindIndex(rankings, ranking => ranking.Login == login);

            SortedList<uint, PlayerRank> result = new SortedList<uint, PlayerRank>();

            // always add the first 3 records, replace non existing records with empty ones
            for (uint i = 1; i <= 3; i++)
            {
                PlayerRank currentRank = rankings.Length >= i ? rankings[i - 1] : new PlayerRank { Login = string.Empty, NickName = string.Empty, Rank = (int)i };
                result.Add(i, currentRank);
            }

            // leave if no more records left
            if (maxRecordsToShow <= 3)
                return result;

            uint amountOfRecordsLeft = maxRecordsToShow - 3;
            uint upperLimitLeft = 4 + ((maxRecordsToReport - 3) / 2) + ((maxRecordsToReport - 3) % 2);
            uint lowerLimitRight = upperLimitLeft;

            if (currentPlayerRankIndex != -1 && currentPlayerRankIndex > 2 && currentPlayerRankIndex < maxRecordsToReport)
            {
                result.Add((uint)(currentPlayerRankIndex + 1), rankings[currentPlayerRankIndex]);
                amountOfRecordsLeft--;

                upperLimitLeft = (uint)currentPlayerRankIndex + 1;
                lowerLimitRight = (uint)(currentPlayerRankIndex + 2);
            }

            List<uint> ranksBeforePlayerRank = new List<uint>();
            for (uint i = 4; i < upperLimitLeft; i++)
                ranksBeforePlayerRank.Add(i);

            List<uint> ranksAfterPlayerRank = new List<uint>();
            for (uint i = lowerLimitRight; i <= maxRecordsToReport; i++)
                ranksAfterPlayerRank.Add(i);

            uint leftAmount = (amountOfRecordsLeft / 2) + (amountOfRecordsLeft % 2);
            uint rightAmount = (amountOfRecordsLeft / 2);

            if (leftAmount > ranksBeforePlayerRank.Count)
            {
                uint diff = leftAmount - (uint)ranksBeforePlayerRank.Count;
                leftAmount = (uint)ranksBeforePlayerRank.Count;
                rightAmount += diff;
            }

            if (rightAmount > ranksAfterPlayerRank.Count)
            {
                uint diff = rightAmount - (uint)ranksAfterPlayerRank.Count;
                rightAmount = (uint)ranksAfterPlayerRank.Count;
                leftAmount += diff;
            }

            uint leftAmountStart = leftAmount, leftAmountEnd = 0;
            uint rightAmountStart = 0, rightAmountEnd = rightAmount;
            if (currentPlayerRankIndex != -1 && currentPlayerRankIndex > 2)
            {
                leftAmountStart = (leftAmount / 2);
                leftAmountEnd = (leftAmount / 2) + (leftAmount % 2);
                rightAmountStart = (rightAmount / 2);
                rightAmountEnd = (rightAmount / 2) + (rightAmount % 2);
            }

            for (int i = 0; i < leftAmountStart; i++)
                result.Add(ranksBeforePlayerRank[i], rankings[ranksBeforePlayerRank[i] - 1]);

            for (int i = ranksBeforePlayerRank.Count - 1; i > (ranksBeforePlayerRank.Count - 1) - leftAmountEnd; i--)
                result.Add(ranksBeforePlayerRank[i], rankings[ranksBeforePlayerRank[i] - 1]);

            for (int i = 0; i < rightAmountStart; i++)
                result.Add(ranksAfterPlayerRank[i], rankings[ranksAfterPlayerRank[i] - 1]);

            for (int i = ranksAfterPlayerRank.Count - 1; i > (ranksAfterPlayerRank.Count - 1) - rightAmountEnd; i--)
                result.Add(ranksAfterPlayerRank[i], rankings[ranksAfterPlayerRank[i] - 1]);

            return result;
        }

        private void HideUI()
        {
            SendEmptyManiaLinkPage(LIVE_RANKING_LIST_MANIA_LINK_PAGE_ID);
        }

        protected override void Dispose(bool connectionLost)
        {
            UpdateTimer.Stop();

            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerFinish -= Callbacks_PlayerFinish;
        }

        #endregion
    }
}