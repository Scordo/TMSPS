using System;
using System.Text;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania
{
    public class DedimaniaUIPlugin : DedimaniaPluginPlugin
    {
        #region Members

        private readonly string _dedimaniaManiaLinkPageID = "DedimaniaRecordPanelID"; //Guid.NewGuid().ToString("N");

        #endregion


        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "Dedimania UI Plugin"; } }
        public override string Description { get { return "Displays dedimania records in a userinterface."; } }
        public override string ShortName { get { return "DedimaniaUI"; } }
        public DedimaniaUISettings Settings { get; private set; }

        #endregion

        protected override void Init()
        {
            Settings = DedimaniaUISettings.ReadFromFile(PluginSettingsFilePath);
            HostPlugin.RankChanged += HostPlugin_RankChanged;
            HostPlugin.RankingsChanged += HostPlugin_RankingsChanged;
            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
        }

        private void Callbacks_EndRace(object sender, Core.Communication.EventArguments.Callbacks.EndRaceEventArgs e)
        {
            if (Settings.ShowDedimaniaRecordUI)
                SendHideDedimaniaRecordManiaLinkPageToAll();
        }

        private void Callbacks_PlayerConnect(object sender, Core.Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            if (Settings.ShowDedimaniaRecordUI)
                SendDedimaniaRecordManiaLinkPageToLogin(e.Login);
        }

        private void HostPlugin_RankingsChanged(object sender, Common.EventArgs<DedimaniaRanking[]> e)
        {
            if (Settings.ShowDedimaniaRecordUI)
                SendDedimaniaRecordManiaLinkPageToAll();
        }

        private void HostPlugin_RankChanged(object sender, RankingChangedEventArgs e)
        {
            if (Settings.ShowMessages)
            {
                if (e.RankChanged)
                {
                    string message = Settings.NewDedimaniaRankMessage.Replace("{[Nickname]}", e.Nickname).Replace("{[Rank]}", e.NewRank.ToString());
                    Context.RPCClient.Methods.ChatSend(message);
                    Context.RPCClient.Methods.SendNotice(message, e.Login, Convert.ToInt32(Settings.NoticeDelayInSeconds));
                }
                else
                {
                    string message = Settings.ImprovedDedimaniaRankMessage.Replace("{[Nickname]}", e.Nickname).Replace("{[Rank]}", e.NewRank.ToString());
                    Context.RPCClient.Methods.ChatSend(message);
                    Context.RPCClient.Methods.SendNotice(message, e.Login, Convert.ToInt32(Settings.NoticeDelayInSeconds));
                }
            }

            if (e.NewRank == 1 && Settings.ShowDedimaniaRecordUI)
            {
                SendDedimaniaRecordManiaLinkPageToAll();
            }
        }

        private void SendHideDedimaniaRecordManiaLinkPageToAll()
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(Settings.DediPanelTemplateInactive.Replace("{[ManiaLinkID]}", _dedimaniaManiaLinkPageID), 0, false);
        }

        private void SendDedimaniaRecordManiaLinkPageToLogin(string login)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetDedimaniaRecordManiaLinkPage(), 0, false);
        }

        private void SendDedimaniaRecordManiaLinkPageToAll()
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(GetDedimaniaRecordManiaLinkPage(), 0, false);
        }

        private string GetDedimaniaRecordManiaLinkPage()
        {
            StringBuilder maniaLinkPage = new StringBuilder(Settings.DediPanelTemplateActive);

            string timeString = "  --.--  ";
            if (HostPlugin.BestTime.HasValue)
            {
                TimeSpan time = TimeSpan.FromMilliseconds(HostPlugin.BestTime.Value);
                timeString = string.Format("{0}:{1}.{2}", time.Minutes, time.Seconds.ToString("00"), (time.Milliseconds / 10).ToString("00"));
            }

            maniaLinkPage.Replace("{[DED]}", timeString);
            maniaLinkPage.Replace("{[ManiaLinkID]}", _dedimaniaManiaLinkPageID);

            return maniaLinkPage.ToString();
        }

        protected override void Dispose()
        {
            HostPlugin.RankChanged -= HostPlugin_RankChanged;
            HostPlugin.RankingsChanged -= HostPlugin_RankingsChanged;
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
        }
    }
}