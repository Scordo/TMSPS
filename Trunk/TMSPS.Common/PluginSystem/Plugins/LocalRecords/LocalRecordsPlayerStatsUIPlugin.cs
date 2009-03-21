using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.ManiaLinking;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class LocalRecordsPlayerStatsUIPlugin : LocalRecordsPluginPlugin
    {
        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "Local Records Player Stats UI Plugin"; } }
        public override string Description { get { return "Displays player related stats in userinterfaces."; } }
        public override string ShortName { get { return "PlayerStatsUI"; } }

        protected PagedUIDialogSettings TopSumsSettings { get; private set; }
        private const string _topSumsManiaLinkPageID = "TopSumsPanelID";
        private PagedDialogActions TopSumsActions { get; set; }

 

        #endregion

        protected override void Init()
        {
            TopSumsActions = new PagedDialogActions(ID, (byte) Area.TopSums);
            TopSumsSettings = PagedUIDialogSettings.ReadFromFile(Path.Combine(PluginDirectory, "TopRecordsTemplate.xml"));

            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            RunCatchLog(() =>
            {
                if (CheckForTopSumsCommand(e))
                    return;
            }, "Error in Callbacks_PlayerChat Method.", true);
        }

        private bool CheckForTopSumsCommand(PlayerChatEventArgs args)
        {
            if (string.Compare(args.Text, "/topsums", StringComparison.InvariantCultureIgnoreCase) != 0 && string.Compare(args.Text, "/summary", StringComparison.InvariantCultureIgnoreCase) != 0)
                return false;

            IEnumerable<TopRankingEntry> rankings = HostPlugin.RankingAdapter.GetTopRankings(0, 18);
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(args.Login, GetTopSumsManiaLinkPage(1, 1, rankings), 0, false);

            return true;
        }


        private string GetTopSumsManiaLinkPage(uint currentPage, uint maxPage, IEnumerable<TopRankingEntry> topRankingEntries)
        {
            string mainTemplateString = TopSumsSettings.SinglePageTemplate;

            if (maxPage > 1)
            {
                if (currentPage == 1)
                    mainTemplateString = TopSumsSettings.FirstPageTemplate;
                else 
                    mainTemplateString = currentPage == maxPage ? TopSumsSettings.LastPageTemplate : TopSumsSettings.MiddlePageTemplate;
            }

            mainTemplateString = ReplaceMessagePlaceHolders(mainTemplateString, TopSumsActions.GetReplaceParameters());

            XElement mainTemplate = XElement.Parse(FormatMessage(mainTemplateString, "ManiaLinkID", _topSumsManiaLinkPageID, "CurrentPage", currentPage.ToString(CultureInfo.InvariantCulture), "MaxPage", maxPage.ToString(CultureInfo.InvariantCulture)));
            XElement entryPlaceHolder = mainTemplate.Descendants("PlayerPlaceHolder").First();
            double currentY = TopSumsSettings.FirstEntryTopMargin;

            XElement lastInsertedNode = entryPlaceHolder;

            foreach (TopRankingEntry rankingEntry in topRankingEntries)
            {
                XElement currentElement = GetRankingEntryElement(rankingEntry, currentY);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= TopSumsSettings.EntryHeight;
            }

            entryPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetRankingEntryElement(TopRankingEntry rankingEntry, double currentY)
        {
            return XElement.Parse
            (
                FormatMessage
                (
                    TopSumsSettings.EntryTemplate,
                    "Y", currentY.ToString(CultureInfo.InvariantCulture),
                    "Position", rankingEntry.Position.ToString(CultureInfo.InvariantCulture),
                    "Nickname", SecurityElement.Escape(rankingEntry.Nickname),
                    "First", rankingEntry.FirstRecords.ToString(CultureInfo.InvariantCulture),
                    "Second", rankingEntry.SecondRecords.ToString(CultureInfo.InvariantCulture),
                    "Third", rankingEntry.ThirdRecords.ToString(CultureInfo.InvariantCulture)
                )
            );
        }

        protected override void OnManiaLinkPageAnswer(string login, int playerID, TMAction action)
        {
            Area area = (Area) action.AreaID;

            switch (area)
            {
                case Area.TopSums:
                    HandleTopSumsManiaLinkResponse(login, playerID, action);
                    break;
            }
        }

        private void HandleTopSumsManiaLinkResponse(string login, int playerID, TMAction action)
        {
            if (!action.IsAreaAction)
                return;

            PagedDialogActions.DefaultDialogAction dialogAction = (PagedDialogActions.DefaultDialogAction) action.AreaActionID;

            switch (dialogAction)
            {
                case PagedDialogActions.DefaultDialogAction.CloseDialog:
                    SendEmptyManiaLinkPageToLogin(login, _topSumsManiaLinkPageID);
                    break;
            }
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
        }

        #region Embedded Types

        private enum Area : byte
        {
            TopSums = 1,
        }

        #endregion

    }
}
