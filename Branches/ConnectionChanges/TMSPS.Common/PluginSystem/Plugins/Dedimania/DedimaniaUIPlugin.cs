﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Configuration;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania
{
    public class DedimaniaUIPlugin : DedimaniaPluginPlugin
    {
        #region Members

        private const string _dedimaniaManiaLinkPageID = "DedimaniaRecordPanelID";
        private const string _dedimaniaRecordListManiaLinkPageID = "DedimaniaRecordListPanelID";

        #endregion

        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "Dedimania UI Plugin"; } }
        public override string Description { get { return "Displays dedimania records in a userinterface."; } }
        public override string ShortName { get { return "DedimaniaUI"; } }
        public DedimaniaUISettings Settings { get; private set; }
        private TimedVolatileExecutionQueue<DedimaniaRanking[]> UpdateListTimer { get; set; }
        private TimedVolatileExecutionQueue<object> UpdateRecordTimer { get; set; }

        #endregion

        #region Constructor

        protected DedimaniaUIPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion

        #region Methods

        protected override void Init()
        {
            Settings = DedimaniaUISettings.ReadFromFile(PluginSettingsFilePath);
            UpdateListTimer = new TimedVolatileExecutionQueue<DedimaniaRanking[]>(TimeSpan.FromSeconds(Settings.UpdateInterval));
            UpdateRecordTimer = new TimedVolatileExecutionQueue<object>(TimeSpan.FromSeconds(Settings.UpdateInterval));

            HostPlugin.RankChanged += HostPlugin_RankChanged;
            HostPlugin.RankingsChanged += HostPlugin_RankingsChanged;
            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
        }

        protected override void Dispose(bool connectionLost)
        {
            UpdateListTimer.Stop();
            UpdateRecordTimer.Stop();

            HostPlugin.RankChanged -= HostPlugin_RankChanged;
            HostPlugin.RankingsChanged -= HostPlugin_RankingsChanged;
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;

            if (!connectionLost)
            {
                SendEmptyManiaLinkPage(_dedimaniaManiaLinkPageID);
                SendEmptyManiaLinkPage(_dedimaniaRecordListManiaLinkPageID);
            }
        }

        private void Callbacks_EndRace(object sender, TMSPS.Core.Communication.EventArguments.Callbacks.EndRaceEventArgs e)
        {
            RunCatchLog(() =>
            {
                UpdateListTimer.Clear();
                UpdateRecordTimer.Clear();

                if (Settings.ShowRecordUI)
                    SendEmptyManiaLinkPage(_dedimaniaManiaLinkPageID);
                    
                if (Settings.ShowRecordListUI && Settings.HideRecordListUIOnFinish)
                    SendEmptyManiaLinkPage(_dedimaniaRecordListManiaLinkPageID);
            }, "Error in Callbacks_EndRace Method.", false);
        }

        private void Callbacks_PlayerConnect(object sender, TMSPS.Core.Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            if (e.Handled)
                return;

            RunCatchLog(() =>
            {
                if (Settings.ShowRecordUI)
                    SendDedimaniaRecordManiaLinkPageToLogin(e.Login);

                if (Settings.ShowRecordListUI)
                    SendRecordListToLogin(e.Login);
            }, "Error in Callbacks_PlayerConnect Method.", false);
        }

        private void HostPlugin_RankingsChanged(object sender, EventArgs<DedimaniaRanking[]> e)
        {
            RunCatchLog(() =>
            {
                if (Settings.ShowRecordUI)
                    UpdateRecordTimer.Enqueue(SendDedimaniaRecordManiaLinkPageToAll, null);
                    //SendDedimaniaRecordManiaLinkPageToAll();

                if (Settings.ShowRecordListUI)
                    UpdateListTimer.Enqueue(SendRecordListToAllPlayers, new List<DedimaniaRanking>(e.Value).ToArray());
                    //SendRecordListToAllPlayers(e.Value);
            }, "Error in HostPlugin_RankingsChanged Method.", false);
        }

        private void HostPlugin_RankChanged(object sender, RankingChangedEventArgs e)
        {
            RunCatchLog(() =>
            {
                if (Settings.ShowMessages)
                {
                    if (e.RankChanged)
                    {
                        string message = FormatMessage(Settings.NewRankMessage, "Nickname", StripTMColorsAndFormatting(e.Nickname), "Rank", e.NewRank.ToString());
                        Context.RPCClient.Methods.ChatSendServerMessage(message);
                        Context.RPCClient.Methods.SendNotice(message, e.Login, Convert.ToInt32(Settings.NoticeDelayInSeconds));
                    }
                    else
                    {
                        string message = FormatMessage(Settings.ImprovedRankMessage, "Nickname", StripTMColorsAndFormatting(e.Nickname), "Rank", e.NewRank.ToString());
                        Context.RPCClient.Methods.ChatSendServerMessage(message);
                        Context.RPCClient.Methods.SendNotice(message, e.Login, Convert.ToInt32(Settings.NoticeDelayInSeconds));
                    }
                }

                if (e.NewRank == 1 && Settings.ShowRecordUI)
                    UpdateRecordTimer.Enqueue(SendDedimaniaRecordManiaLinkPageToAll, null);
                //SendDedimaniaRecordManiaLinkPageToAll();
            }, "Error in HostPlugin_RankChanged Method.", true);
        }

        private void SendRecordListToAllPlayers(DedimaniaRanking[] rankings)
        {
            if (rankings != null && rankings.Length > 0 && HostPlugin.IsDedimaniaResponsive)
            {
                if (PlayersCount < Settings.StaticModeStartLimit)
                {
                    foreach (PlayerSettings playerSettings in Context.PlayerSettings.GetAllAsList())
                    {
                        string maniaLinkPageContent = GetRecordListManiaLinkPage(rankings, playerSettings.Login);
                        string hash = maniaLinkPageContent.ToHash();

                        if (playerSettings.ManiaLinkPageHashStore.Get(_dedimaniaRecordListManiaLinkPageID) == hash)
                            continue;
                        
                        SetManiaLinkPageHash(playerSettings.Login, _dedimaniaRecordListManiaLinkPageID, hash);
                        Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(playerSettings.Login, maniaLinkPageContent, 0, false);
                    }
                }
                else
                {
                    Context.RPCClient.Methods.SendDisplayManialinkPage(GetRecordListManiaLinkPage(rankings, null), 0, false);
                }
            }
            else
                Context.RPCClient.Methods.SendDisplayManialinkPage(GetRecordListManiaLinkPage(new DedimaniaRanking[] { }, null), 0, false);
        }

        private void SendRecordListToLogin(string login)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetRecordListManiaLinkPage(HostPlugin.Rankings, PlayersCount < Settings.StaticModeStartLimit ? login : null), 0, false);
        }

        private string GetRecordListManiaLinkPage(DedimaniaRanking[] rankings, string login)
        {
            // show at least top 3 
            int recordsToShow = Convert.ToInt32(Math.Max(Math.Min(Settings.MaxRecordsToShow, rankings.Length), 3));

            double totalHeight = Math.Abs(Settings.RecordListPlayerToContainerMarginY * 2) + Math.Abs(Settings.RecordListPlayerRecordHeight * recordsToShow) + Math.Abs(Settings.RecordListTop3Gap) + Math.Abs(Settings.RecordListPlayerEndMargin);

            XElement mainTemplate = XElement.Parse(Settings.RecordListMainTemplate.Replace("{[ManiaLinkID]}", _dedimaniaRecordListManiaLinkPageID).Replace("{[ContainerHeight}}", totalHeight.ToString(Context.Culture)));
            XElement rankingPlaceHolder = mainTemplate.Descendants("RankingPlaceHolder").First();
            double currentY = Settings.RecordListPlayerStartMargin;

            XElement lastInsertedNode = rankingPlaceHolder;

            SortedList<uint, DedimaniaRanking> localRankings = GetRankingsToShow(rankings, login, Settings.MaxRecordsToShow, DedimaniaSettings.MAX_RECORDS_TO_REPORT);

            foreach (KeyValuePair<uint, DedimaniaRanking> localRanking in localRankings)
            {
                XElement currentElement = GetPlayerRecordElement(localRanking.Value, currentY, localRanking.Key, login);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= Settings.RecordListPlayerRecordHeight;

                if (localRanking.Key == 3 && localRankings.Count > 3)
                    currentY -= Settings.RecordListTop3Gap;
            }

            rankingPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        public static SortedList<uint, DedimaniaRanking> GetRankingsToShow(DedimaniaRanking[] rankings, string login, uint maxRecordsToShow, uint maxRecordsToReport)
        {
            // show at least the top 3
            maxRecordsToShow = (uint)Math.Max(Math.Min(Math.Min(maxRecordsToShow, maxRecordsToReport), rankings.Length), 3);
            maxRecordsToReport = (uint)Math.Min(rankings.Length, maxRecordsToReport);

            // set maxRecordsToShow to amount of existing rankings when the amoutn of rankings is less than the value of maxRecordsToShow
            if (rankings.Length < maxRecordsToShow && rankings.Length > 3)
                maxRecordsToShow = Convert.ToUInt32(rankings.Length);

            int currentPlayerRankIndex = Array.FindIndex(rankings, ranking => ranking.Login == login);

            SortedList<uint, DedimaniaRanking> result = new SortedList<uint, DedimaniaRanking>();

            // always add the first 3 records, replace non existing records with empty ones
            for (uint i = 1; i <= 3; i++)
            {
                DedimaniaRanking currentRank = rankings.Length >= i ? rankings[i - 1] : new DedimaniaRanking(string.Empty, string.Empty, 0, DateTime.MinValue);
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
                leftAmountStart = (leftAmount/2);
                leftAmountEnd = (leftAmount/2) + (leftAmount%2);
                rightAmountStart = (rightAmount / 2);
                rightAmountEnd = (rightAmount / 2) + (rightAmount % 2);
            }

            for (int i = 0; i < leftAmountStart; i++)
                result.Add(ranksBeforePlayerRank[i], rankings[ranksBeforePlayerRank[i] - 1]);

            for (int i = ranksBeforePlayerRank.Count - 1; i > (ranksBeforePlayerRank.Count-1) - leftAmountEnd; i--)
                result.Add(ranksBeforePlayerRank[i], rankings[ranksBeforePlayerRank[i] - 1]);

            for (int i = 0; i < rightAmountStart; i++)
                result.Add(ranksAfterPlayerRank[i], rankings[ranksAfterPlayerRank[i] - 1]);

            for (int i = ranksAfterPlayerRank.Count - 1; i > (ranksAfterPlayerRank.Count - 1) - rightAmountEnd; i--)
                result.Add(ranksAfterPlayerRank[i], rankings[ranksAfterPlayerRank[i] - 1]);

            return result;
        }

        private XElement GetPlayerRecordElement(DedimaniaRanking rankingInfo, double currentY, uint currentRank, string login)
        {
            string templateXML = Settings.RecordListRecordTemplate;

            if (rankingInfo.Login == login)
                templateXML = Settings.RecordListRecordHighlightTemplate;
            else if (currentRank <= 3)
                templateXML = Settings.RecordListTop3RecordTemplate;

            TimeSpan time = TimeSpan.FromMilliseconds(rankingInfo.TimeOrScore);

            StringBuilder playerRecordXml = new StringBuilder(templateXML);
            playerRecordXml.Replace("{[Y]}", currentY.ToString(CultureInfo.InvariantCulture));
            playerRecordXml.Replace("{[Rank]}", currentRank + ".");
            playerRecordXml.Replace("{[TimeOrScore]}", rankingInfo.TimeOrScore == 0 ? "  --.--  " : string.Format("{0}:{1}.{2}", time.Minutes, time.Seconds.ToString("00"), (time.Milliseconds / 10).ToString("00")));

            string nickname = SecurityElement.Escape(rankingInfo.Nickname);

            if (Settings.StripNickFormatting)
                nickname = StripTMColorsAndFormatting(nickname);

            playerRecordXml.Replace("{[Nickname]}", nickname);

            return XElement.Parse(playerRecordXml.ToString());
        }

        private void SendDedimaniaRecordManiaLinkPageToLogin(string login)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetDedimaniaRecordManiaLinkPage(), 0, false);
        }

        private void SendDedimaniaRecordManiaLinkPageToAll(object dummy)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(GetDedimaniaRecordManiaLinkPage(), 0, false);
        }

        private string GetDedimaniaRecordManiaLinkPage()
        {
            StringBuilder maniaLinkPage = new StringBuilder(Settings.DediPanelTemplateActive);

            string timeString = "  --.--  ";
            if (HostPlugin.BestTime.HasValue && HostPlugin.IsDedimaniaResponsive)
            {
                TimeSpan time = TimeSpan.FromMilliseconds(HostPlugin.BestTime.Value);
                timeString = string.Format("{0}:{1}.{2}", time.Minutes, time.Seconds.ToString("00"), (time.Milliseconds / 10).ToString("00"));
            }

            maniaLinkPage.Replace("{[DED]}", timeString);
            maniaLinkPage.Replace("{[ManiaLinkID]}", _dedimaniaManiaLinkPageID);

            return maniaLinkPage.ToString();
        }

        #endregion
    }
}