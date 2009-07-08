using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.PluginSystem.Configuration;
using Version = System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class LocalRecordsUIPlugin : LocalRecordsPluginPlugin
    {
        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "Local Records UI Plugin"; } }
        public override string Description { get { return "Displays statistics and local records in a userinterface."; } }
        public override string ShortName { get { return "LocalRecordsUI"; } }

        protected LocalRecordsUISettings Settings { get; private set; }
        private uint? LocalBestTimeOrScore { get; set; }
        private const string _pbManiaLinkPageID = "PBPanelID";
        private const string _localRecordManiaLinkPageID = "LocalRecordPanelID"; 
        private const string _localRecordListManiaLinkPageID = "LocalRecordListPanelID"; 
        private RankEntry[] LastRankings { get; set; }
        private TimedVolatileExecutionQueue<RankEntry[]> UpdateListTimer { get; set; }
        private TimedVolatileExecutionQueue<string> UpdateLocalRecordTimer { get; set; }

        #endregion

        #region Methods

        protected override void Init()
        {
            LastRankings = new RankEntry[] { };
            Settings = LocalRecordsUISettings.ReadFromFile(PluginSettingsFilePath);

            if (Settings.MaxRecordsToShow > HostPlugin.Settings.MaxRecordsToReport)
                Settings.MaxRecordsToShow = HostPlugin.Settings.MaxRecordsToReport;

            UpdateListTimer = new TimedVolatileExecutionQueue<RankEntry[]>(TimeSpan.FromSeconds(Settings.UpdateInterval));
            UpdateLocalRecordTimer = new TimedVolatileExecutionQueue<string>(TimeSpan.FromSeconds(Settings.UpdateInterval));

            //SendPBManiaLinkPageToAll(null);
            HostPlugin.PlayerNewRecord += HostPlugin_PlayerNewRecord;
            HostPlugin.LocalRecordsDetermined += HostPlugin_LocalRecordsDetermined;
            HostPlugin.PlayerWins += HostPlugin_PlayerWins;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
        }

        protected override void Dispose(bool connectionLost)
        {
            UpdateListTimer.Stop();
            UpdateLocalRecordTimer.Stop();

            HostPlugin.PlayerNewRecord -= HostPlugin_PlayerNewRecord;
            HostPlugin.LocalRecordsDetermined -= HostPlugin_LocalRecordsDetermined;
            HostPlugin.PlayerWins -= HostPlugin_PlayerWins;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;

            if (!connectionLost)
            {
                SendEmptyManiaLinkPage(_localRecordListManiaLinkPageID);
                SendEmptyManiaLinkPage(_localRecordManiaLinkPageID);
                SendEmptyManiaLinkPage(_pbManiaLinkPageID);
            }
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            RunCatchLog(() =>
            {
                if (CheckForServerRankCommand(e))
                    return;

                if (CheckForNextServerRankCommand(e))
                    return;

                if (CheckForInfoCommand(e))
                    return;

                if (CheckForSelectUndrivenTracksCommand(e))
                    return;
            }, "Error in Callbacks_PlayerChat Method.", true);
        }

        private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
        {
            if (e.Handled)
                return;

            RunCatchLog(() =>
            {
                if (Settings.ShowPBUserInterface)
                {
                    uint? personalBest = HostPlugin.RecordAdapter.GetBestTime(e.Login, HostPlugin.CurrentChallengeID);
                    SendPBManiaLinkPage(e.Login, personalBest);
                }

                if (Settings.ShowLocalRecordUserInterface)
                    SendLocalRecordManiaLinkToLogin(e.Login);

                if (Settings.ShowLocalRecordListUserInterface)
                {
                    string maniaLinkPageContent = GetRecordListManiaLinkPage(LastRankings, PlayersCount < Settings.StaticModeStartLimit ? e.Login : null);
                    string hash = maniaLinkPageContent.ToHash();
                    SetManiaLinkPageHash(e.Login, _localRecordListManiaLinkPageID, hash);

                    Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(e.Login, maniaLinkPageContent, 0, false);
                }

                if (Settings.ShowMessages)
                    SendServerRankMessageToLogin(e.Login);
            }, "Error in Callbacks_PlayerConnect Method.", true);
        }

        private void Callbacks_EndRace(object sender, EndRaceEventArgs e)
        {
            RunCatchLog(() =>
            {
                UpdateListTimer.Clear();
                UpdateLocalRecordTimer.Clear();

                if (Settings.ShowPBUserInterface)
                    SendEmptyManiaLinkPage(_pbManiaLinkPageID);

                if (Settings.ShowLocalRecordUserInterface)
                    SendEmptyManiaLinkPage(_localRecordManiaLinkPageID);

                if (Settings.ShowLocalRecordListUserInterface && Settings.HideRecordListUIOnFinish)
                    SendEmptyManiaLinkPage(_localRecordListManiaLinkPageID);
            }, "Error in Callbacks_EndRace Method.", true);
        }

        private void HostPlugin_PlayerNewRecord(object sender, PlayerNewRecordEventArgs e)
        {
            RunCatchLog(() =>
            {
                if (!e.OldPosition.HasValue)
                {
                    if (Settings.ShowMessages)
                    {
                        string message = FormatMessage(Settings.FirstLocalRankMessage, "Nickname", StripTMColorsAndFormatting(e.Nickname), "Rank", e.NewPosition.ToString());
                        Context.RPCClient.Methods.SendNotice(message, e.Login, Convert.ToInt32(Settings.NoticeDelayInSeconds));
                        Context.RPCClient.Methods.ChatSendServerMessage(message);
                    }
                }
                else if (e.NewPosition > e.OldPosition)
                {
                    if (Settings.ShowMessages)
                    {
                        string message = FormatMessage(Settings.NewLocalRankMessage, "Nickname", StripTMColorsAndFormatting(e.Nickname), "OldRank", e.OldPosition.ToString(), "NewRank", e.NewPosition.ToString());
                        Context.RPCClient.Methods.SendNotice(message, e.Login, Convert.ToInt32(Settings.NoticeDelayInSeconds));
                        Context.RPCClient.Methods.ChatSendServerMessage(message);
                    }
                }
                else
                {
                    if (Settings.ShowMessages)
                    {
                        string message = FormatMessage(Settings.ImprovedLocalRankMessage, "Nickname", StripTMColorsAndFormatting(e.Nickname), "Rank", e.NewPosition.ToString());
                        Context.RPCClient.Methods.SendNotice(message, e.Login, Convert.ToInt32(Settings.NoticeDelayInSeconds));
                        Context.RPCClient.Methods.ChatSendServerMessage(message);
                    }
                }

                if (Settings.ShowPBUserInterface)
                    SendPBManiaLinkPage(e.Login, Convert.ToUInt32(e.TimeOrScore));

                if (LocalBestTimeOrScore == null || e.TimeOrScore < LocalBestTimeOrScore)
                {
                    LocalBestTimeOrScore = Convert.ToUInt32(e.TimeOrScore);

                    if (Settings.ShowLocalRecordUserInterface)
                        UpdateLocalRecordTimer.Enqueue(SendLocalRecordManiaLinkPageToAll, null);
                    //SendLocalRecordManiaLinkPageToAll();
                }

                if (Settings.ShowLocalRecordUserInterface)
                    UpdateListTimer.Enqueue(SendRecordListToAllPlayers, HostPlugin.LocalRecords);
                //SendRecordListToAllPlayers(HostPlugin.LocalRecords);
            }, "Error in HostPlugin_PlayerNewRecord Method.", true);
        }

        private void HostPlugin_PlayerWins(object sender, PlayerWinEventArgs e)
        {
            RunCatchLog(() =>
            {
                if (Settings.ShowMessages)
                    SendFormattedMessageToLogin(e.RankingInfo.Login, Settings.WinMessage, "Wins", e.Wins.ToString());
            }, "Error in HostPlugin_PlayerWins Method.", true);
        }

        private void HostPlugin_LocalRecordsDetermined(object sender, EventArgs<RankEntry[]> e)
        {
            RunCatchLog(() =>
            {
                LastRankings = e.Value;
                LocalBestTimeOrScore = e.Value.Length > 0 ? (uint?)e.Value[0].TimeOrScore : null;

                if (Settings.ShowPBUserInterface)
                    SendPBManiaLinkPageToAll(e.Value);

                if (Settings.ShowLocalRecordUserInterface)
                    SendLocalRecordManiaLinkPageToAll();

                if (Settings.ShowLocalRecordUserInterface)
                    SendRecordListToAllPlayers(e.Value);
            }, "Error in HostPlugin_LocalRecordsDetermined Method.", true);
        }

        private bool CheckForServerRankCommand(PlayerChatEventArgs args)
        {
            if (string.Compare(args.Text, CommandOrRight.SERVER_RANK1, StringComparison.InvariantCultureIgnoreCase) != 0 && string.Compare(args.Text, CommandOrRight.SERVER_RANK2, StringComparison.InvariantCultureIgnoreCase) != 0)
                return false;

            SendServerRankMessageToLogin(args.Login);

            return true;
        }

        private bool CheckForNextServerRankCommand(PlayerChatEventArgs args)
        {
            if (string.Compare(args.Text, CommandOrRight.NEXT_SERVER_RANK1, StringComparison.InvariantCultureIgnoreCase) != 0 && string.Compare(args.Text, CommandOrRight.NEXT_SERVER_RANK2, StringComparison.InvariantCultureIgnoreCase) != 0)
                return false;

            SendNextServerRankMessageToLogin(args.Login);

            return true;
        }

        private bool CheckForInfoCommand(PlayerChatEventArgs args)
        {
            if (string.Compare(args.Text, CommandOrRight.Info, StringComparison.InvariantCultureIgnoreCase) != 0 &&
                string.Compare(args.Text, CommandOrRight.Wins, StringComparison.InvariantCultureIgnoreCase) != 0 &&
                string.Compare(args.Text, CommandOrRight.Played, StringComparison.InvariantCultureIgnoreCase) != 0 &&
                string.Compare(args.Text, CommandOrRight.Visit, StringComparison.InvariantCultureIgnoreCase) != 0)
                return false;

            SendInfoMessageToLogin(args.Login);

            return true;
        }

        private bool CheckForSelectUndrivenTracksCommand(PlayerChatEventArgs args)
        {
            if (!ServerCommand.Parse(args.Text).IsMainCommandAnyOf(CommandOrRight.SELECT_UNDRIVEN_TRACKS))
                return false;

            if (!LoginHasRight(args.Login, true, CommandOrRight.SELECT_UNDRIVEN_TRACKS))
                return true;

            SelectUndrivenTracks(args.Login);

            return true;
        }

        private void SelectUndrivenTracks(string login)
        {
            ThreadPool.QueueUserWorkItem(SelectUndrivenTracks, login);
        }

        private void SelectUndrivenTracks(object state)
        {
            RunCatchLog(() =>
            {
                string login = (string)state;
                HashSet<string> drivenChallengeUIDs = GetDrivenChallengeUIDs(login);
                List<string> undrivenChallengeFilenames = GetUndrivenChallengeFilenames(drivenChallengeUIDs);
                GenericResponse<int> chooseNextChallengeListResponse = null;    

                if (undrivenChallengeFilenames.Count > 0)
                {
                    chooseNextChallengeListResponse = Context.RPCClient.Methods.ChooseNextChallengeList(undrivenChallengeFilenames);

                    if (LogFaultResponse(chooseNextChallengeListResponse, "SelectUndrivenTracks"))
                        return;
                }

                if (chooseNextChallengeListResponse.Value > 0)
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#MessageStyle]}You have not driven {[#HighlightStyle]}{[Amount]}{[#MessageStyle]} track(s). Those tracks will be the next in the track cycle.", "Amount", chooseNextChallengeListResponse.Value.ToString());
                else
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#MessageStyle]}You have driven all maps.");
            }, "Error in SelectUndrivenTracks Method.", true);
        }

        private List<string> GetUndrivenChallengeFilenames(HashSet<string> uniqueIDs)
        {
            GenericListResponse<ChallengeListSingleInfo> getChallGenericListResponse = Context.RPCClient.Methods.GetChallengeList();

            if (getChallGenericListResponse.Erroneous)
            {

                return new List<string>();
            }

            List<ChallengeListSingleInfo> undrivenTracks = getChallGenericListResponse.Value.FindAll(c => !uniqueIDs.Contains(c.UId));

            return undrivenTracks.ConvertAll(c => c.FileName);
        }

        private HashSet<string> GetDrivenChallengeUIDs(string login)
        {
            return new HashSet<string>(HostPlugin.ChallengeAdapter.GetDrivenUniqueTrackIDs(login));
        }

        private void SendServerRankMessageToLogin(string login)
        {
            ThreadPool.QueueUserWorkItem(SendServerRankMessageToLogin, login);
        }

        private void SendNextServerRankMessageToLogin(string login)
        {
            ThreadPool.QueueUserWorkItem(SendNextServerRankMessageToLogin, login);
        }

        private void SendServerRankMessageToLogin(object state)
        {
            string login = (string) state;
            Ranking ranking = HostPlugin.RankingAdapter.Deserialize_ByLogin(login);

            if (ranking != null)
                SendFormattedMessageToLogin(login, Settings.RankingMessage, "Rank", ranking.CurrentRank.ToString("F0", CultureInfo.InvariantCulture), "Average", ranking.AverageRank.ToString("F1", CultureInfo.InvariantCulture), "Score", ranking.Score.ToString("F1", CultureInfo.InvariantCulture), "Tracks", ranking.RecordsCount.ToString("F0", CultureInfo.InvariantCulture), "TracksCount", ranking.ChallengesCount.ToString("F0", CultureInfo.InvariantCulture));
        }

        private void SendNextServerRankMessageToLogin(object state)
        {
            string login = (string)state;
            Ranking ranking = HostPlugin.RankingAdapter.GetNextRank(login);

            if (ranking != null)
                SendFormattedMessageToLogin(login, Settings.NextRankMessage, "Nickname", StripTMColorsAndFormatting(ranking.Nickname), "Rank", ranking.CurrentRank.ToString("F0", CultureInfo.InvariantCulture), "Average", ranking.AverageRank.ToString("F1", CultureInfo.InvariantCulture), "Score", ranking.Score.ToString("F1", CultureInfo.InvariantCulture), "Tracks", ranking.RecordsCount.ToString("F0", CultureInfo.InvariantCulture), "TracksCount", ranking.ChallengesCount.ToString("F0", CultureInfo.InvariantCulture));
            else
                SendFormattedMessageToLogin(login, Settings.NoBetterRankMessage);
        }

        private void SendInfoMessageToLogin(string login)
        {
            Player player = HostPlugin.PlayerAdapter.Deserialize(login);

            if (player != null)
                SendFormattedMessageToLogin(login, Settings.InfoMessage, "Wins", player.Wins.ToString(CultureInfo.InvariantCulture), "Played", player.TimePlayed.TotalHours.ToString("F0", CultureInfo.InvariantCulture) + "h", "Created", player.Created.ToShortDateString());
        }

        private void SendRecordListToAllPlayers(RankEntry[] rankings)
        {
            if (rankings != null && rankings.Length > 0)
            {
                if (PlayersCount < Settings.StaticModeStartLimit)
                {
                    foreach (PlayerSettings playerSettings in Context.PlayerSettings.GetAllAsList())
                    {
                        string maniaLinkPageContent = GetRecordListManiaLinkPage(rankings, playerSettings.Login);
                        string hash = maniaLinkPageContent.ToHash();

                        if (playerSettings.ManiaLinkPageHashStore.Get(_localRecordListManiaLinkPageID) != hash)
                        {
                            SetManiaLinkPageHash(playerSettings.Login, _localRecordListManiaLinkPageID, hash);
                            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(playerSettings.Login, maniaLinkPageContent, 0, false);
                        }
                    }
                }
                else
                {
                    Context.RPCClient.Methods.SendDisplayManialinkPage(GetRecordListManiaLinkPage(rankings, null), 0, false);
                }
            }
            else
                Context.RPCClient.Methods.SendDisplayManialinkPage(GetRecordListManiaLinkPage(new RankEntry[] { }, null), 0, false);
        }

        private void SendPBManiaLinkPageToAll(RankEntry[] rankEntries)
        {
            if (rankEntries == null)
                rankEntries = new RankEntry[] { };

            foreach (PlayerSettings playerSettings in Context.PlayerSettings.GetAllAsList())
            {
                RankEntry rank = Array.Find(rankEntries, rankEntry => rankEntry.Login == playerSettings.Login);

                uint? personalBest = rank == null ? null : (uint?)rank.TimeOrScore;

                if (!personalBest.HasValue)
                    personalBest = HostPlugin.RecordAdapter.GetBestTime(playerSettings.Login, HostPlugin.CurrentChallengeID);


                SendPBManiaLinkPage(playerSettings.Login, personalBest);
            }
        }

        private void SendPBManiaLinkPage(string login, uint? personalBestTimeOrScore)
        {
            StringBuilder maniaLinkPage = new StringBuilder(Settings.PBPanelTemplate);

            string pbValue = "  --.--  ";
            if (personalBestTimeOrScore.HasValue)
            {
                TimeSpan pbTime = TimeSpan.FromMilliseconds(personalBestTimeOrScore.Value);
                pbValue = string.Format("{0}:{1}.{2}", pbTime.Minutes, pbTime.Seconds.ToString("00"), (pbTime.Milliseconds / 10).ToString("00"));
            }

            maniaLinkPage.Replace("{[PB]}", pbValue);
            maniaLinkPage.Replace("{[ManiaLinkID]}", _pbManiaLinkPageID);

            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, maniaLinkPage.ToString(), 0, false);
        }

        private void SendLocalRecordManiaLinkToLogin(string login)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetLocalRecordManiaLinkPage(), 0, false);
        }

        private void SendLocalRecordManiaLinkPageToAll(string dummy)
        {
            RunCatchLog(SendLocalRecordManiaLinkPageToAll, "Error in SendLocalRecordManiaLinkPageToAll Method.", true);
        }

        private void SendLocalRecordManiaLinkPageToAll()
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(GetLocalRecordManiaLinkPage(), 0, false);
        }

        private string GetLocalRecordManiaLinkPage()
        {
            StringBuilder maniaLinkPage = new StringBuilder(Settings.LocalRecordPanelTemplate);

            string timeString = "  --.--  ";
            if (LocalBestTimeOrScore.HasValue)
            {
                TimeSpan time = TimeSpan.FromMilliseconds(LocalBestTimeOrScore.Value);
                timeString = string.Format("{0}:{1}.{2}", time.Minutes, time.Seconds.ToString("00"), (time.Milliseconds / 10).ToString("00"));
            }

            maniaLinkPage.Replace("{[LCL]}", timeString);
            maniaLinkPage.Replace("{[ManiaLinkID]}", _localRecordManiaLinkPageID);

            return maniaLinkPage.ToString();
        }

        private string GetRecordListManiaLinkPage(RankEntry[] rankings, string login)
        {
            // show at least top 3 
            int recordsToShow = Convert.ToInt32(Math.Max(Math.Min(Settings.MaxRecordsToShow, rankings.Length), 3));

            double totalHeight = Math.Abs(Settings.RecordListPlayerToContainerMarginY * 2) + Math.Abs(Settings.RecordListPlayerRecordHeight * recordsToShow) + Math.Abs(Settings.RecordListTop3Gap) + Math.Abs(Settings.RecordListPlayerEndMargin);

            XElement mainTemplate = XElement.Parse(Settings.RecordListMainTemplate.Replace("{[ManiaLinkID]}", _localRecordListManiaLinkPageID).Replace("{[ContainerHeight}}", totalHeight.ToString(Context.Culture)));
            XElement rankingPlaceHolder = mainTemplate.Descendants("RankingPlaceHolder").First();
            double currentY = Settings.RecordListPlayerStartMargin;

            XElement lastInsertedNode = rankingPlaceHolder;

            SortedList<uint, RankEntry> localRankings = GetRankingsToShow(rankings, login, Settings.MaxRecordsToShow, HostPlugin.Settings.MaxRecordsToReport);

            foreach (KeyValuePair<uint, RankEntry> localRanking in localRankings)
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

        private XElement GetPlayerRecordElement(RankEntry rankingInfo, double currentY, uint currentRank, string login)
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

        public static SortedList<uint, RankEntry> GetRankingsToShow(RankEntry[] rankings, string login, uint maxRecordsToShow, uint maxRecordsToReport)
        {
            // show at least the top 3
            maxRecordsToShow = (uint)Math.Max(Math.Min(Math.Min(maxRecordsToShow, maxRecordsToReport), rankings.Length), 3);
            maxRecordsToReport = (uint)Math.Min(rankings.Length, maxRecordsToReport);

            // set maxRecordsToShow to amount of existing rankings when the amoutn of rankings is less than the value of maxRecordsToShow
            if (rankings.Length < maxRecordsToShow && rankings.Length > 3)
                maxRecordsToShow = Convert.ToUInt32(rankings.Length);

            int currentPlayerRankIndex = Array.FindIndex(rankings, ranking => ranking.Login == login);

            SortedList<uint, RankEntry> result = new SortedList<uint, RankEntry>();

            // always add the first 3 records, replace non existing records with empty ones
            for (uint i = 1; i <= 3; i++)
            {
                RankEntry currentRank = rankings.Length >= i ? rankings[i - 1] : new RankEntry((ushort)i, string.Empty, string.Empty, 0);
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

        #endregion
    }
}