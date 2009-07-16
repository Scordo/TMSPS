using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.ManiaLinking;
using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.Core.PluginSystem.Plugins.AdminPlayer
{
    public partial class AdminPlayerPlugin
    {
        #region Constants

        private const string BLACKLIST_PANEL_ID = "AdminPlayerBlackListPanelID";

        #endregion

        #region Properties

        private PagedUIDialogSettingsBase<PagedUIDialogSettings> BlackListSettings { get; set; }
        private PagedDialogActions BlackListActions { get; set; }

        #endregion

        #region Methods

        private void HandleBlackListAreaActions(string login, TMAction areaAction)
        {
            if (areaAction.IsAreaAction)
            {
                PagedDialogActions.DefaultDialogAction action = (PagedDialogActions.DefaultDialogAction)areaAction.AreaActionID;

                switch (action)
                {
                    case PagedDialogActions.DefaultDialogAction.CloseDialog:
                        GetPluginSettings(login).AreaSettings.Reset((byte)Area.BlackListArea);
                        SendEmptyManiaLinkPageToLogin(login, BLACKLIST_PANEL_ID);
                        break;
                    case PagedDialogActions.DefaultDialogAction.FirstPage:
                        SendBlackListPageToLogin(login, 0);
                        break;
                    case PagedDialogActions.DefaultDialogAction.PrevPage:
                        ushort prevPageIndex = Convert.ToUInt16(Math.Max(0, GetAreaSettings(login, (byte)Area.BlackListArea).CurrentDialogPageIndex - 1));
                        SendBlackListPageToLogin(login, prevPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.NextPage:

                        ushort nextPageIndex = Convert.ToUInt16(GetAreaSettings(login, (byte)Area.BlackListArea).CurrentDialogPageIndex + 1);
                        SendBlackListPageToLogin(login, nextPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.LastPage:
                        SendBlackListPageToLogin(login, null);
                        break;
                }
            }
            else
            {
                BlackListRowAction action = (BlackListRowAction)areaAction.RowActionID;

                switch (action)
                {
                    case BlackListRowAction.RemovePlayer:
                        RemoveBlackListPlayer(login, areaAction.RowIndex);
                        break;
                }
            }
        }

        private void RemoveBlackListPlayer(string login, byte rowIndex)
        {
            Dictionary<byte, string> visibleLogins = (Dictionary<byte, string>)GetAreaSettings(login, (byte)Area.BlackListArea).CustomData;

            if (visibleLogins == null || !visibleLogins.ContainsKey(rowIndex))
                return;

            string loginToRemove = visibleLogins[rowIndex];
            Context.CorePlugin.RemoveBlackListLogin(login, loginToRemove);
            SendBlackListPageToLogin(login, GetAreaSettings(login, (byte)Area.BlackListArea).CurrentDialogPageIndex);
        }

        private void SendBlackListPageToLogin(string login, uint? pageIndex)
        {
            if (!LoginHasRight(login, true, Command.Blacklist))
                return;

            const int MAX_BlackLIST_SIZE = 1000;

            GenericListResponse<LoginResponse> BlackListResponse = Context.RPCClient.Methods.GetBlackList(MAX_BlackLIST_SIZE, 0);

            if (BlackListResponse.Erroneous)
            {
                Logger.Error("Error retrieving black list: " + BlackListResponse.Fault.FaultMessage);
                return;
            }

            List<LoginResponse> BlackList = BlackListResponse.Value;

            uint maxPageIndex = Convert.ToUInt32(Math.Max(0, Math.Ceiling((double)BlackList.Count / BlackListSettings.MaxEntriesPerPage) - 1));

            if (!pageIndex.HasValue)
                pageIndex = maxPageIndex;

            pageIndex = Convert.ToUInt16(Math.Min(Math.Max(0, (int)pageIndex), maxPageIndex));
            GetAreaSettings(login, (byte)Area.BlackListArea).CurrentDialogPageIndex = (ushort)pageIndex;

            int entriesToSkip = Convert.ToInt32(pageIndex * BlackListSettings.MaxEntriesPerPage);
            int startPosition = entriesToSkip + 1;

            List<PlayerListEntry> playerListEntriesToShow = BlackList.Skip(entriesToSkip)
                .Take(Convert.ToInt32(BlackListSettings.MaxEntriesPerPage))
                .Select((l, i) => new PlayerListEntry((ushort)(startPosition + i), GetNickname(l.Login), l.Login))
                .ToList();

            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetBlackListManiaLinkPage(pageIndex.Value + 1, maxPageIndex + 1, playerListEntriesToShow), 0, false);

            Dictionary<byte, string> rowSettings = new Dictionary<byte, string>();
            byte rowIndex = 0;
            playerListEntriesToShow.ForEach(p => { rowSettings[rowIndex] = p.Login; rowIndex++; });
            GetAreaSettings(login, (byte)Area.BlackListArea).CustomData = rowSettings;
        }

        private string GetBlackListManiaLinkPage(uint currentPage, uint maxPage, IEnumerable<PlayerListEntry> logins)
        {
            string mainTemplateString = BlackListSettings.SinglePageTemplate;

            if (maxPage > 1)
            {
                if (currentPage == 1)
                    mainTemplateString = BlackListSettings.FirstPageTemplate;
                else
                    mainTemplateString = currentPage == maxPage ? BlackListSettings.LastPageTemplate : BlackListSettings.MiddlePageTemplate;
            }

            mainTemplateString = ReplaceMessagePlaceHolders(mainTemplateString, BlackListActions.GetReplaceParameters());

            XElement mainTemplate = XElement.Parse(FormatMessage(mainTemplateString, "ManiaLinkID", BLACKLIST_PANEL_ID, "CurrentPage", currentPage.ToString(CultureInfo.InvariantCulture), "MaxPage", maxPage.ToString(CultureInfo.InvariantCulture)));
            XElement entryPlaceHolder = mainTemplate.Descendants("PlayerPlaceHolder").First();
            double currentY = BlackListSettings.FirstEntryTopMargin;

            XElement lastInsertedNode = entryPlaceHolder;

            byte rowIndex = 0;

            foreach (PlayerListEntry playerListEntry in logins)
            {
                XElement currentElement = GetBlackListPlayerElement(playerListEntry, currentY, rowIndex);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= BlackListSettings.EntryHeight;
                rowIndex++;
            }

            entryPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetBlackListPlayerElement(PlayerListEntry playerEntry, double currentY, byte rowIndex)
        {
            return XElement.Parse
            (
                FormatMessage
                (
                    BlackListSettings.EntryTemplate,
                    "Y", currentY.ToString(CultureInfo.InvariantCulture),
                    "Position", playerEntry.Position.ToString(CultureInfo.InvariantCulture),
                    "Nickname", SecurityElement.Escape(playerEntry.Nickname ?? "[" + playerEntry.Login + "]"),
                    "Login", SecurityElement.Escape(playerEntry.Login),
                    "RemoveActionID", TMAction.CalculateActionID(ID, (byte)Area.BlackListArea, rowIndex, (byte)BlackListRowAction.RemovePlayer).ToString(CultureInfo.InvariantCulture)
                )
            );
        }

        #endregion

        #region Embedded Types

        private enum BlackListRowAction
        {
            RemovePlayer = 1,
        }

        #endregion
    }
}