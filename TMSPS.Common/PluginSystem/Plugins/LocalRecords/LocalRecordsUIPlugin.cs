using System;
using System.Globalization;

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

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = LocalRecordsUISettings.ReadFromFile(PluginSettingsFilePath);

            if (Settings.MaxRecordsToShow > HostPlugin.Settings.MaxRecordsToReport)
                Settings.MaxRecordsToShow = HostPlugin.Settings.MaxRecordsToReport;

            HostPlugin.PlayerVoted += HostPlugin_PlayerVoted;
            HostPlugin.PlayerNewRecord += HostPlugin_PlayerNewRecord;
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
        }

        protected override void Dispose()
        {
            HostPlugin.PlayerVoted += HostPlugin_PlayerVoted;
            HostPlugin.PlayerNewRecord += HostPlugin_PlayerNewRecord;
        }

        #endregion
    }
}