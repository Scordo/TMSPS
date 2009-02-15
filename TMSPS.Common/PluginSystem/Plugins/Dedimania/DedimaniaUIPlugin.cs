using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using TMSPS.Core.Communication.ProxyTypes;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania
{
    public class DedimaniaUIPlugin : DedimaniaPluginPlugin
    {
        #region Members

        private readonly string _dedimaniaManiaLinkPageID = "DedimaniaRecordPanelID"; //Guid.NewGuid().ToString("N");
        private readonly string _dedimaniaRecordListManiaLinkPageID = "DedimaniaRecordListPanelID"; //Guid.NewGuid().ToString("N");

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
            {
                SendDedimaniaRecordManiaLinkPageToLogin(e.Login);
                Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(e.Login, GetRecordListManiaLinkPage(HostPlugin.Rankings, e.Login), 0, false);
            }
        }

        private void SendRecordListToAllPlayers(DedimaniaRanking[] rankings)
        {
            List<PlayerInfo> players = DedimaniaPlugin.GetPlayerList(this);

            if (players == null)
                return;

            if (rankings != null && rankings.Length > 0)
            {
                foreach (PlayerInfo playerInfo in players)
                {
                    Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(playerInfo.Login, GetRecordListManiaLinkPage(HostPlugin.Rankings, playerInfo.Login), 0, false);
                }
            }
            else
                Context.RPCClient.Methods.SendDisplayManialinkPage(GetRecordListManiaLinkPage(new DedimaniaRanking[] {}, null), 0, false);
        }

        private void HostPlugin_RankingsChanged(object sender, Common.EventArgs<DedimaniaRanking[]> e)
        {
            if (Settings.ShowDedimaniaRecordUI)
            {
                SendDedimaniaRecordManiaLinkPageToAll();
                SendRecordListToAllPlayers(e.Value);
            }
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

            for (int i = 1; i <= 3; i++)
            {
                DedimaniaRanking currentRank = rankings.Length >= i ? rankings[i - 1] : new DedimaniaRanking(string.Empty, string.Empty, 0, DateTime.MinValue);
                XElement currentElement = GetPlayerRecordElement(Settings.RecordListTop3RecordTemplate, currentRank, currentY, i, login);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= Settings.RecordListPlayerRecordHeight;
            }

            currentY -= Settings.RecordListTop3Gap;

            for (int i = 4; i <= recordsToShow; i++)
            {
                DedimaniaRanking currentRank = rankings[i - 1];
                XElement currentElement = GetPlayerRecordElement(Settings.RecordListRecordTemplate, currentRank, currentY, i, login);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= Settings.RecordListPlayerRecordHeight;
            }


            rankingPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetPlayerRecordElement(string templateXML, DedimaniaRanking ranking, double currentY, int currentRank, string login)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(ranking.TimeOrScore);


            StringBuilder playerRecordXml = new StringBuilder(ranking.Login != login ? templateXML : Settings.RecordListRecordHighlightTemplate);
            playerRecordXml.Replace("{[Y]}", currentY.ToString(CultureInfo.InvariantCulture));
            playerRecordXml.Replace("{[Rank]}", currentRank + ".");
            playerRecordXml.Replace("{[TimeOrScore]}", ranking.TimeOrScore == 0 ? "  --.--  " : string.Format("{0}:{1}.{2}", time.Minutes, time.Seconds.ToString("00"), (time.Milliseconds / 10).ToString("00")));
            playerRecordXml.Replace("{[Nickname]}", SecurityElement.Escape(ranking.Nickname));

            return XElement.Parse(playerRecordXml.ToString());
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