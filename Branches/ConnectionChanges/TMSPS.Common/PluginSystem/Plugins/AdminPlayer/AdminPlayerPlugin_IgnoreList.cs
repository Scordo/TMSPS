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

        private const string IGNORELIST_PANEL_ID = "AdminPlayerIgnoreListPanelID";

        #endregion

        #region Properties

        private PagedUIDialogSettingsBase<PagedUIDialogSettings> IgnoreListSettings { get; set; }
        private PagedDialogActions IgnoreListActions { get; set; }

        #endregion

        #region Methods

        private void HandleIgnoreListAreaActions(string login, TMAction areaAction)
        {
            if (areaAction.IsAreaAction)
            {
                PagedDialogActions.DefaultDialogAction action = (PagedDialogActions.DefaultDialogAction)areaAction.AreaActionID;

                switch (action)
                {
                    case PagedDialogActions.DefaultDialogAction.CloseDialog:
                        GetPluginSettings(login).AreaSettings.Reset((byte)Area.IgnoreListArea);
                        SendEmptyManiaLinkPageToLogin(login, IGNORELIST_PANEL_ID);
                        break;
                    case PagedDialogActions.DefaultDialogAction.FirstPage:
                        SendIgnoreListPageToLogin(login, 0);
                        break;
                    case PagedDialogActions.DefaultDialogAction.PrevPage:
                        ushort prevPageIndex = Convert.ToUInt16(Math.Max(0, GetAreaSettings(login, (byte)Area.IgnoreListArea).CurrentDialogPageIndex - 1));
                        SendIgnoreListPageToLogin(login, prevPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.NextPage:

                        ushort nextPageIndex = Convert.ToUInt16(GetAreaSettings(login, (byte)Area.IgnoreListArea).CurrentDialogPageIndex + 1);
                        SendIgnoreListPageToLogin(login, nextPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.LastPage:
                        SendIgnoreListPageToLogin(login, null);
                        break;
                }
            }
            else
            {
                IgnoreListRowAction action = (IgnoreListRowAction)areaAction.RowActionID;

                switch (action)
                {
                    case IgnoreListRowAction.RemovePlayer:
                        RemoveIgnoreListPlayer(login, areaAction.RowIndex);
                        break;
                }
            }
        }

        private void RemoveIgnoreListPlayer(string login, byte rowIndex)
        {
            Dictionary<byte, string> visibleLogins = (Dictionary<byte, string>)GetAreaSettings(login, (byte)Area.IgnoreListArea).CustomData;

            if (visibleLogins == null || !visibleLogins.ContainsKey(rowIndex))
                return;

            string loginToRemove = visibleLogins[rowIndex];
            Context.CorePlugin.UnIgnoreLogin(login, loginToRemove);
            SendIgnoreListPageToLogin(login, GetAreaSettings(login, (byte)Area.IgnoreListArea).CurrentDialogPageIndex);
        }

        private void SendIgnoreListPageToLogin(string login, uint? pageIndex)
        {
            if (!LoginHasRight(login, true, Command.Ignore))
                return;

            const int MAX_IGNORELIST_SIZE = 1000;

            GenericListResponse<LoginResponse> ignoreListResponse = Context.RPCClient.Methods.GetIgnoreList(MAX_IGNORELIST_SIZE, 0);

            if (ignoreListResponse.Erroneous)
            {
                Logger.Error("Error retrieving IgnoreList: " + ignoreListResponse.Fault.FaultMessage);
                return;
            }

            List<LoginResponse> ignoreList = ignoreListResponse.Value;

            uint maxPageIndex = Convert.ToUInt32(Math.Max(0, Math.Ceiling((double)ignoreList.Count / IgnoreListSettings.MaxEntriesPerPage) - 1));

            if (!pageIndex.HasValue)
                pageIndex = maxPageIndex;

            pageIndex = Convert.ToUInt16(Math.Min(Math.Max(0, (int)pageIndex), maxPageIndex));
            GetAreaSettings(login, (byte)Area.IgnoreListArea).CurrentDialogPageIndex = (ushort)pageIndex;

            int entriesToSkip = Convert.ToInt32(pageIndex * IgnoreListSettings.MaxEntriesPerPage);
            int startPosition = entriesToSkip + 1;

            List<PlayerListEntry> playerListEntriesToShow = ignoreList.Skip(entriesToSkip)
                .Take(Convert.ToInt32(IgnoreListSettings.MaxEntriesPerPage))
                .Select((l, i) => new PlayerListEntry((ushort)(startPosition + i), GetNickname(l.Login), l.Login))
                .ToList();

            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetIgnoreListManiaLinkPage(pageIndex.Value + 1, maxPageIndex + 1, playerListEntriesToShow), 0, false);

            Dictionary<byte, string> rowSettings = new Dictionary<byte, string>();
            byte rowIndex = 0;
            playerListEntriesToShow.ForEach(p => { rowSettings[rowIndex] = p.Login; rowIndex++; });
            GetAreaSettings(login, (byte)Area.IgnoreListArea).CustomData = rowSettings;
        }

        private string GetIgnoreListManiaLinkPage(uint currentPage, uint maxPage, IEnumerable<PlayerListEntry> logins)
        {
            string mainTemplateString = IgnoreListSettings.SinglePageTemplate;

            if (maxPage > 1)
            {
                if (currentPage == 1)
                    mainTemplateString = IgnoreListSettings.FirstPageTemplate;
                else
                    mainTemplateString = currentPage == maxPage ? IgnoreListSettings.LastPageTemplate : IgnoreListSettings.MiddlePageTemplate;
            }

            mainTemplateString = ReplaceMessagePlaceHolders(mainTemplateString, IgnoreListActions.GetReplaceParameters());

            XElement mainTemplate = XElement.Parse(FormatMessage(mainTemplateString, "ManiaLinkID", IGNORELIST_PANEL_ID, "CurrentPage", currentPage.ToString(CultureInfo.InvariantCulture), "MaxPage", maxPage.ToString(CultureInfo.InvariantCulture)));
            XElement entryPlaceHolder = mainTemplate.Descendants("PlayerPlaceHolder").First();
            double currentY = IgnoreListSettings.FirstEntryTopMargin;

            XElement lastInsertedNode = entryPlaceHolder;

            byte rowIndex = 0;

            foreach (PlayerListEntry playerListEntry in logins)
            {
                XElement currentElement = GetIgnoreListPlayerElement(playerListEntry, currentY, rowIndex);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= IgnoreListSettings.EntryHeight;
                rowIndex++;
            }

            entryPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetIgnoreListPlayerElement(PlayerListEntry playerEntry, double currentY, byte rowIndex)
        {
            return XElement.Parse
            (
                FormatMessage
                (
                    IgnoreListSettings.EntryTemplate,
                    "Y", currentY.ToString(CultureInfo.InvariantCulture),
                    "Position", playerEntry.Position.ToString(CultureInfo.InvariantCulture),
                    "Nickname", SecurityElement.Escape(playerEntry.Nickname ?? "[" + playerEntry.Login + "]"),
                    "Login", SecurityElement.Escape(playerEntry.Login),
                    "RemoveActionID", TMAction.CalculateActionID(ID, (byte)Area.IgnoreListArea, rowIndex, (byte)IgnoreListRowAction.RemovePlayer).ToString(CultureInfo.InvariantCulture)
                )
            );
        }

        #endregion

        #region Embedded Types

        private enum IgnoreListRowAction
        {
            RemovePlayer = 1,
        }

        #endregion
    }
}