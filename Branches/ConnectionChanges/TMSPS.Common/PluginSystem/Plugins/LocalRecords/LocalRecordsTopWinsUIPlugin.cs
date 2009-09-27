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
using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class LocalRecordsTopWinsUIPlugin : LocalRecordsPluginPlugin
    {
        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "Local Records Top Wins UI Plugin"; } }
        public override string Description { get { return "Displays the top wins in an userinterfaces."; } }
        public override string ShortName { get { return "TopWinsUI"; } }

        protected PagedUIDialogSettingsBase<PagedUIDialogSettings> TopWinsSettings { get; private set; }
        private const string _topWinsManiaLinkPageID = "TopWinsPanelID";
        private PagedDialogActions TopWinsActions { get; set; }



        #endregion

        #region Constructor

        protected LocalRecordsTopWinsUIPlugin(string pluginDirectory)
            : base(pluginDirectory)
        {

        }

        #endregion

        protected override void Init()
        {
            TopWinsActions = new PagedDialogActions(ID, (byte)Area.TopWins);
            TopWinsSettings = PagedUIDialogSettingsBase<PagedUIDialogSettings>.ReadFromFile(Path.Combine(PluginDirectory, "TopWinsTemplate.xml"));

            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
        }

        public override IEnumerable<CommandHelp> CommandHelpList
        {
            get
            {
                return new[]
                {
                    new CommandHelp(Command.TopWins, "Shows a list of all players order by their amount of wins descending.", "/topwins", "/topwins"),
                };
            }
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            RunCatchLog(() =>
            {
                if (CheckForTopWinsCommand(e))
                    return;
            }, "Error in Callbacks_PlayerChat Method.", true);
        }

        private bool CheckForTopWinsCommand(PlayerChatEventArgs args)
        {
            if (!ServerCommand.Parse(args.Text).Is(Command.TopWins))
                return false;

            SendTopWinsPageToLogin(args.Login, 0);

            return true;
        }

        private void SendTopWinsPageToLogin(string login, ushort pageIndex)
        {
            GetAreaSettings(login, (byte)Area.TopWins).CurrentDialogPageIndex = pageIndex;
            uint[] pageIndeces = GetPageIndices(pageIndex, TopWinsSettings.MaxEntriesPerPage);
            PagedList<IndexedPlayer> players;

            using (IPlayerAdapter playerAdapter = HostPlugin.AdapterProvider.GetPlayerAdapter())
            {
                players = playerAdapter.DeserializeListByWins((int)pageIndeces[0], (int)pageIndeces[1]);
            }

            uint maxPage = Convert.ToUInt32(Math.Ceiling((double)players.VirtualCount / TopWinsSettings.MaxEntriesPerPage));

            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetTopRanksManiaLinkPage(Convert.ToUInt16(pageIndex + 1), maxPage, players), 0, false);
        }


        private string GetTopRanksManiaLinkPage(uint currentPage, uint maxPage, IEnumerable<IndexedPlayer> topWinners)
        {
            string mainTemplateString = TopWinsSettings.SinglePageTemplate;

            if (maxPage > 1)
            {
                if (currentPage == 1)
                    mainTemplateString = TopWinsSettings.FirstPageTemplate;
                else
                    mainTemplateString = currentPage == maxPage ? TopWinsSettings.LastPageTemplate : TopWinsSettings.MiddlePageTemplate;
            }

            mainTemplateString = ReplaceMessagePlaceHolders(mainTemplateString, TopWinsActions.GetReplaceParameters());

            XElement mainTemplate = XElement.Parse(FormatMessage(mainTemplateString, "ManiaLinkID", _topWinsManiaLinkPageID, "CurrentPage", currentPage.ToString(CultureInfo.InvariantCulture), "MaxPage", maxPage.ToString(CultureInfo.InvariantCulture)));
            XElement entryPlaceHolder = mainTemplate.Descendants("PlayerPlaceHolder").First();
            double currentY = TopWinsSettings.FirstEntryTopMargin;

            XElement lastInsertedNode = entryPlaceHolder;

            foreach (IndexedPlayer player in topWinners)
            {
                XElement currentElement = GetRankingElement(player, currentY);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= TopWinsSettings.EntryHeight;
            }

            entryPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetRankingElement(IndexedPlayer player, double currentY)
        {
            return XElement.Parse
            (
                FormatMessage
                (
                    TopWinsSettings.EntryTemplate,
                    "Y", currentY.ToString(CultureInfo.InvariantCulture),
                    "Rank", player.RowNumber.ToString(CultureInfo.InvariantCulture),
                    "Nickname", SecurityElement.Escape(player.Nickname),
                    "Wins", player.Wins.ToString(CultureInfo.InvariantCulture)
                )
            );
        }

        protected override void OnManiaLinkPageAnswer(string login, int playerID, TMAction action)
        {
            Area area = (Area)action.AreaID;

            switch (area)
            {
                case Area.TopWins:
                    HandleTopWinsManiaLinkResponse(login, action);
                    break;
            }
        }

        private void HandleTopWinsManiaLinkResponse(string login, TMAction action)
        {
            if (!action.IsAreaAction)
                return;

            PagedDialogActions.DefaultDialogAction dialogAction = (PagedDialogActions.DefaultDialogAction)action.AreaActionID;

            switch (dialogAction)
            {
                case PagedDialogActions.DefaultDialogAction.CloseDialog:
                    GetPluginSettings(login).AreaSettings.Reset((byte)Area.TopWins);
                    SendEmptyManiaLinkPageToLogin(login, _topWinsManiaLinkPageID);
                    break;
                case PagedDialogActions.DefaultDialogAction.FirstPage:
                    SendTopWinsPageToLogin(login, 0);
                    break;
                case PagedDialogActions.DefaultDialogAction.PrevPage:
                    ushort prevPageIndex = Convert.ToUInt16(Math.Max(0, GetAreaSettings(login, (byte)Area.TopWins).CurrentDialogPageIndex - 1));
                    SendTopWinsPageToLogin(login, prevPageIndex);
                    break;
                case PagedDialogActions.DefaultDialogAction.NextPage:
                    ushort nextPageIndex = Convert.ToUInt16(GetAreaSettings(login, (byte)Area.TopWins).CurrentDialogPageIndex + 1);
                    SendTopWinsPageToLogin(login, nextPageIndex);
                    break;
                case PagedDialogActions.DefaultDialogAction.LastPage:
                    uint topRanksCount;
                    using (IRankingAdapter rankingAdapter = HostPlugin.AdapterProvider.GetRankingAdapter())
                    {
                        topRanksCount = rankingAdapter.GetRanksCount();
                    }

                    ushort lastPageIndex = Convert.ToUInt16(Math.Max(0, Math.Ceiling((double)topRanksCount / TopWinsSettings.MaxEntriesPerPage) - 1));
                    SendTopWinsPageToLogin(login, lastPageIndex);
                    break;
            }
        }

        #region Embedded Types

        private enum Area : byte
        {
            TopWins = 1,
        }

        #endregion

    }
}
