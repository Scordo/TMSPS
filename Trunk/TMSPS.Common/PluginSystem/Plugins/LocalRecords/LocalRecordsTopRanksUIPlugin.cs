using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.ManiaLinking;
using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class LocalRecordsTopRanksUIPlugin : LocalRecordsPluginPlugin
    {
        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "Local Records Player Stats UI Plugin"; } }
        public override string Description { get { return "Displays the top ranks in an userinterfaces."; } }
        public override string ShortName { get { return "TopRanksUI"; } }

        protected PagedUIDialogSettingsBase<PagedUIDialogSettings> TopRanksSettings { get; private set; }
        private const string _topRanksManiaLinkPageID = "TopRanksPanelID";
        private PagedDialogActions TopRanksActions { get; set; }



        #endregion

        protected override void Init()
        {
            TopRanksActions = new PagedDialogActions(ID, (byte)Area.TopRanks);
            TopRanksSettings = PagedUIDialogSettingsBase<PagedUIDialogSettings>.ReadFromFile(Path.Combine(PluginDirectory, "TopRanksTemplate.xml"));

            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            RunCatchLog(() =>
            {
                if (CheckForTopRanksCommand(e))
                    return;
            }, "Error in Callbacks_PlayerChat Method.", true);
        }

        private bool CheckForTopRanksCommand(PlayerChatEventArgs args)
        {
            if (!ServerCommand.Parse(args.Text).Is(Command.TopRanks))
                return false;

            SendTopRanksPageToLogin(args.Login, 0);

            return true;
        }

        private void SendTopRanksPageToLogin(string login, ushort pageIndex)
        {
            GetAreaSettings(login, (byte)Area.TopRanks).CurrentDialogPageIndex = pageIndex;
            uint[] pageIndeces = GetPageIndices(pageIndex, TopRanksSettings.MaxEntriesPerPage);
            uint topRanksCount = HostPlugin.RankingAdapter.GetRanksCount();
            uint maxPage = Convert.ToUInt32(Math.Ceiling((double)topRanksCount / TopRanksSettings.MaxEntriesPerPage));

            IEnumerable<Ranking> rankings = HostPlugin.RankingAdapter.Deserialize_PagedList(pageIndeces[0], pageIndeces[1]);
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetTopRanksManiaLinkPage(Convert.ToUInt16(pageIndex + 1), maxPage, rankings), 0, false);
        }


        private string GetTopRanksManiaLinkPage(uint currentPage, uint maxPage, IEnumerable<Ranking> topRankings)
        {
            string mainTemplateString = TopRanksSettings.SinglePageTemplate;

            if (maxPage > 1)
            {
                if (currentPage == 1)
                    mainTemplateString = TopRanksSettings.FirstPageTemplate;
                else
                    mainTemplateString = currentPage == maxPage ? TopRanksSettings.LastPageTemplate : TopRanksSettings.MiddlePageTemplate;
            }

            mainTemplateString = ReplaceMessagePlaceHolders(mainTemplateString, TopRanksActions.GetReplaceParameters());

            XElement mainTemplate = XElement.Parse(FormatMessage(mainTemplateString, "ManiaLinkID", _topRanksManiaLinkPageID, "CurrentPage", currentPage.ToString(CultureInfo.InvariantCulture), "MaxPage", maxPage.ToString(CultureInfo.InvariantCulture)));
            XElement entryPlaceHolder = mainTemplate.Descendants("RankingPlaceHolder").First();
            double currentY = TopRanksSettings.FirstEntryTopMargin;

            XElement lastInsertedNode = entryPlaceHolder;

            foreach (Ranking ranking in topRankings)
            {
                XElement currentElement = GetRankingElement(ranking, currentY);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= TopRanksSettings.EntryHeight;
            }

            entryPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetRankingElement(Ranking ranking, double currentY)
        {
            return XElement.Parse
            (
                FormatMessage
                (
                    TopRanksSettings.EntryTemplate,
                    "Y", currentY.ToString(CultureInfo.InvariantCulture),
                    "Rank", ranking.CurrentRank.ToString(CultureInfo.InvariantCulture),
                    "Nickname", SecurityElement.Escape(ranking.Nickname),
                    "Score", ranking.Score.ToString("F1", CultureInfo.InvariantCulture),
                    "AVG", ranking.AverageRank.ToString("F1", CultureInfo.InvariantCulture),
                    "Tracks", ranking.RecordsCount.ToString(CultureInfo.InvariantCulture)
                )
            );
        }

        protected override void OnManiaLinkPageAnswer(string login, int playerID, TMAction action)
        {
            Area area = (Area)action.AreaID;

            switch (area)
            {
                case Area.TopRanks:
                    HandleTopRanksManiaLinkResponse(login, playerID, action);
                    break;
            }
        }

        private void HandleTopRanksManiaLinkResponse(string login, int playerID, TMAction action)
        {
            if (!action.IsAreaAction)
                return;

            PagedDialogActions.DefaultDialogAction dialogAction = (PagedDialogActions.DefaultDialogAction)action.AreaActionID;

            switch (dialogAction)
            {
                case PagedDialogActions.DefaultDialogAction.CloseDialog:
                    GetPluginSettings(login).AreaSettings.Reset((byte)Area.TopRanks);
                    SendEmptyManiaLinkPageToLogin(login, _topRanksManiaLinkPageID);
                    break;
                case PagedDialogActions.DefaultDialogAction.FirstPage:
                    SendTopRanksPageToLogin(login, 0);
                    break;
                case PagedDialogActions.DefaultDialogAction.PrevPage:
                    ushort prevPageIndex = Convert.ToUInt16(Math.Max(0, GetAreaSettings(login, (byte)Area.TopRanks).CurrentDialogPageIndex - 1));
                    SendTopRanksPageToLogin(login, prevPageIndex);
                    break;
                case PagedDialogActions.DefaultDialogAction.NextPage:
                    ushort nextPageIndex = Convert.ToUInt16(GetAreaSettings(login, (byte)Area.TopRanks).CurrentDialogPageIndex + 1);
                    SendTopRanksPageToLogin(login, nextPageIndex);
                    break;
                case PagedDialogActions.DefaultDialogAction.LastPage:
                    uint topRanksCount = HostPlugin.RankingAdapter.GetRanksCount();
                    ushort lastPageIndex = Convert.ToUInt16(Math.Max(0, Math.Ceiling((double)topRanksCount / TopRanksSettings.MaxEntriesPerPage) - 1));
                    SendTopRanksPageToLogin(login, lastPageIndex);
                    break;
            }
        }

        #region Embedded Types

        private enum Area : byte
        {
            TopRanks = 1,
        }

        #endregion

    }
}
