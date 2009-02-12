using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMSPS.Core.Communication.ProxyTypes;
using Version=System.Version;

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
        private readonly string _pbManiaLinkPageID = "PBPanelID";//Guid.NewGuid().ToString("N");
        private readonly string _localRecordManiaLinkPageID = "LocalRecordPanelID"; //Guid.NewGuid().ToString("N");

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = LocalRecordsUISettings.ReadFromFile(PluginSettingsFilePath);

            if (Settings.MaxRecordsToShow > HostPlugin.Settings.MaxRecordsToReport)
                Settings.MaxRecordsToShow = HostPlugin.Settings.MaxRecordsToReport;

            //SendPBManiaLinkPageToAll(null);
            HostPlugin.PlayerVoted += HostPlugin_PlayerVoted;
            HostPlugin.PlayerNewRecord += HostPlugin_PlayerNewRecord;
            HostPlugin.LocalRecordsDetermined += HostPlugin_LocalRecordsDetermined;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
        }

        private void Callbacks_PlayerConnect(object sender, Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            if (Settings.ShowPBUserInterface)
            {
                uint? personalBest = HostPlugin.RecordAdapter.GetBestTime(e.Login, HostPlugin.CurrentChallengeID);
                SendPBManiaLinkPage(e.Login, personalBest);
            }

            if (Settings.ShowLocalRecordUserInterface)
                SendLocalRecordManiaLinkToLogin(e.Login);
        }

        private void HostPlugin_LocalRecordsDetermined(object sender, Common.EventArgs<RankEntry[]> e)
        {
            LocalBestTimeOrScore = e.Value.Length > 0 ? (uint?) e.Value[0].TimeOrScore : null;

            if (Settings.ShowPBUserInterface)
                SendPBManiaLinkPageToAll(e.Value);

            if (Settings.ShowLocalRecordUserInterface)
                SendLocalRecordManiaLinkPageToAll();
        }

        private void SendPBManiaLinkPageToAll(RankEntry[] rankEntries)
        {
            List<PlayerInfo> players = HostPlugin.GetPlayerList();

            if (players == null)
                return;

            if (rankEntries == null)
                rankEntries = new RankEntry[]{};

            foreach (PlayerInfo playerInfo in players)
            {
                RankEntry rank = Array.Find(rankEntries, rankEntry => rankEntry.Login == playerInfo.Login);

                uint? personalBest = rank == null ? null : (uint?) rank.TimeOrScore;

                if (!personalBest.HasValue)
                    personalBest = HostPlugin.RecordAdapter.GetBestTime(playerInfo.Login, HostPlugin.CurrentChallengeID);

                
                SendPBManiaLinkPage(playerInfo.Login, personalBest);
            }
        }

        private void SendPBManiaLinkPage(string login, uint? personalBestTimeOrScore)
        {
            StringBuilder maniaLinkPage = new StringBuilder(Settings.PBPanelTemplateActive);

            string pbValue = "  --.--  ";
            if (personalBestTimeOrScore.HasValue)
            {
                TimeSpan pbTime = TimeSpan.FromMilliseconds(personalBestTimeOrScore.Value);
                pbValue = string.Format("{0}:{1}.{2}", pbTime.Minutes, pbTime.Seconds, pbTime.Milliseconds / 10);
            }

            maniaLinkPage.Replace("{[PB]}", pbValue);
            maniaLinkPage.Replace("{[ManiaLinkID]}", _pbManiaLinkPageID);

            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, maniaLinkPage.ToString(), 0, false);
        }

        private void SendLocalRecordManiaLinkToLogin(string login)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetLocalRecordManiaLinkPage(), 0, false);
        }

        private void SendLocalRecordManiaLinkPageToAll()
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(GetLocalRecordManiaLinkPage(), 0, false);
        }

        private string GetLocalRecordManiaLinkPage()
        {
            StringBuilder maniaLinkPage = new StringBuilder(Settings.LocalRecordPanelTemplateActive);

            string timeString = "  --.--  ";
            if (LocalBestTimeOrScore.HasValue)
            {
                TimeSpan time = TimeSpan.FromMilliseconds(LocalBestTimeOrScore.Value);
                timeString = string.Format("{0}:{1}.{2}", time.Minutes, time.Seconds, time.Milliseconds / 10);
            }

            maniaLinkPage.Replace("{[LCL]}", timeString);
            maniaLinkPage.Replace("{[ManiaLinkID]}", _localRecordManiaLinkPageID);

            return maniaLinkPage.ToString();
        }

        private void SendHideLocalRecordManiaLinkPageToAll()
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(Settings.LocalRecordPanelTemplateInactive.Replace("{[ManiaLinkID]}", _localRecordManiaLinkPageID), 0, false);
        }

        private void SendHidePBManiaLinkPageToAll()
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(Settings.PBPanelTemplateInactive.Replace("{[ManiaLinkID]}", _pbManiaLinkPageID), 0, false);
        }


        private void Callbacks_EndRace(object sender, Communication.EventArguments.Callbacks.EndRaceEventArgs e)
        {
            if (Settings.ShowPBUserInterface)
                SendHidePBManiaLinkPageToAll();

            if (Settings.ShowLocalRecordUserInterface)
                SendHideLocalRecordManiaLinkPageToAll();
        }

        private void HostPlugin_PlayerVoted(object sender, PlayerVoteEventArgs e)
        {
            if (Settings.ShowMessages)
            {
                string message = Settings.VoteAcceptedMessage.Replace("{[AverageVote]}", e.AverageVoteValue.ToString("F", CultureInfo.InvariantCulture));
                Context.RPCClient.Methods.ChatSendToLogin(message, e.Login);
            }  
        }

        private void HostPlugin_PlayerNewRecord(object sender, PlayerNewRecordEventArgs e)
        {
            if (!e.OldPosition.HasValue)
            {
                if (Settings.ShowMessages)
                {
                    string message = Settings.FirstLocalRankMessage.Replace("{[Nickname]}", e.PlayerInfo.NickName).Replace("{[Rank]}", e.NewPosition.ToString());
                    Context.RPCClient.Methods.SendNotice(message, e.PlayerInfo.Login, Convert.ToInt32(Settings.NoticeDelayInSeconds));
                    Context.RPCClient.Methods.ChatSend(message);
                }
            }
            else if (e.NewPosition > e.OldPosition)
            {
                if (Settings.ShowMessages)
                {
                    string message = Settings.NewLocalRankMessage.Replace("{[Nickname]}", e.PlayerInfo.NickName).Replace("{[OldRank]}", e.OldPosition.ToString()).Replace("{[NewRank]}", e.NewPosition.ToString());
                    Context.RPCClient.Methods.SendNotice(message, e.PlayerInfo.Login, Convert.ToInt32(Settings.NoticeDelayInSeconds));
                    Context.RPCClient.Methods.ChatSend(message);
                }
            }
            else
            {
                if (Settings.ShowMessages)
                {
                    string message = Settings.ImprovedLocalRankMessage.Replace("{[Nickname]}", e.PlayerInfo.NickName).Replace("{[Rank]}", e.NewPosition.ToString());
                    Context.RPCClient.Methods.SendNotice(message, e.PlayerInfo.Login, Convert.ToInt32(Settings.NoticeDelayInSeconds));
                    Context.RPCClient.Methods.ChatSend(message);
                }
            }

            if (Settings.ShowPBUserInterface)
                SendPBManiaLinkPage(e.PlayerInfo.Login, Convert.ToUInt32(e.TimeOrScore));

            if (LocalBestTimeOrScore == null || e.TimeOrScore < LocalBestTimeOrScore)
            {
                LocalBestTimeOrScore = Convert.ToUInt32(e.TimeOrScore);

                if (Settings.ShowLocalRecordUserInterface)
                    SendLocalRecordManiaLinkPageToAll();
            }
        }

        protected override void Dispose()
        {
            HostPlugin.PlayerVoted += HostPlugin_PlayerVoted;
            HostPlugin.PlayerNewRecord += HostPlugin_PlayerNewRecord;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
        }

        #endregion
    }
}