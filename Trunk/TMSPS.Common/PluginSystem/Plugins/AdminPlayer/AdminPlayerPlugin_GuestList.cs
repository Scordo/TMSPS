using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.ManiaLinking;
using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.Core.PluginSystem.Plugins.AdminPlayer
{
    public partial class AdminPlayerPlugin
    {
        #region Constants

        private const string GUESTLIST_PANEL_ID = "AdminPlayerGuestListPanelID";

        #endregion

        #region Properties

        private PagedUIDialogSettingsBase<PagedUIDialogSettings> GuestListSettings { get; set; }
        private PagedDialogActions GuestListActions { get; set; }

        #endregion

        #region Methods

        private void HandleGuestListAreaActions(string login, TMAction areaAction)
        {
            if (areaAction.IsAreaAction)
            {
                PagedDialogActions.DefaultDialogAction action = (PagedDialogActions.DefaultDialogAction)areaAction.AreaActionID;

                switch (action)
                {
                    case PagedDialogActions.DefaultDialogAction.CloseDialog:
                        GetPluginSettings(login).AreaSettings.Reset((byte)Area.GuestListArea);
                        SendEmptyManiaLinkPageToLogin(login, GUESTLIST_PANEL_ID);
                        break;
                    case PagedDialogActions.DefaultDialogAction.FirstPage:
                        SendGuestListPageToLogin(login, 0);
                        break;
                    case PagedDialogActions.DefaultDialogAction.PrevPage:
                        ushort prevPageIndex = Convert.ToUInt16(Math.Max(0, GetAreaSettings(login, (byte)Area.GuestListArea).CurrentDialogPageIndex - 1));
                        SendGuestListPageToLogin(login, prevPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.NextPage:

                        ushort nextPageIndex = Convert.ToUInt16(GetAreaSettings(login, (byte)Area.GuestListArea).CurrentDialogPageIndex + 1);
                        SendGuestListPageToLogin(login, nextPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.LastPage:
                        SendGuestListPageToLogin(login, null);
                        break;
                }
            }
            else
            {
                GuestListRowAction action = (GuestListRowAction)areaAction.RowActionID;

                switch (action)
                {
                    case GuestListRowAction.RemovePlayer:
                        RemoveGuestListPlayer(login, areaAction.RowIndex);
                        break;
                }
            }
        }

        private void RemoveGuestListPlayer(string login, byte rowIndex)
        {
            Dictionary<byte, string> visibleLogins = (Dictionary<byte, string>)GetAreaSettings(login, (byte)Area.GuestListArea).CustomData;

            if (visibleLogins == null || !visibleLogins.ContainsKey(rowIndex))
                return;

            string loginToRemove = visibleLogins[rowIndex];
            Context.CorePlugin.RemoveGuestLogin(login, loginToRemove);
            SendGuestListPageToLogin(login, GetAreaSettings(login, (byte)Area.GuestListArea).CurrentDialogPageIndex);
        }

        private void SendGuestListPageToLogin(string login, uint? pageIndex)
        {
            if (!LoginHasAnyRight(login, true, TMSPSCorePlugin.COMMAND_ADD_GUEST))
                return;

            const int MAX_GUESTLIST_SIZE = 1000;

            GenericListResponse<LoginResponse> guestListResponse = Context.RPCClient.Methods.GetGuestList(MAX_GUESTLIST_SIZE, 0);

            if (guestListResponse.Erroneous)
            {
                Logger.Error("Error retrieving guest list: " + guestListResponse.Fault.FaultMessage);
                return;
            }

            List<LoginResponse> guestList = guestListResponse.Value;

            uint maxPageIndex = Convert.ToUInt32(Math.Max(0, Math.Ceiling((double)guestList.Count / GuestListSettings.MaxEntriesPerPage) - 1));

            if (!pageIndex.HasValue)
                pageIndex = maxPageIndex;

            pageIndex = Convert.ToUInt16(Math.Min(Math.Max(0, (int)pageIndex), maxPageIndex));
            GetAreaSettings(login, (byte)Area.GuestListArea).CurrentDialogPageIndex = (ushort)pageIndex;

            int entriesToSkip = Convert.ToInt32(pageIndex * GuestListSettings.MaxEntriesPerPage);
            int startPosition = entriesToSkip + 1;

            List<PlayerListEntry> playerListEntriesToShow = guestList.Skip(entriesToSkip)
                .Take(Convert.ToInt32(GuestListSettings.MaxEntriesPerPage))
                .Select((l, i) => new PlayerListEntry((ushort)(startPosition + i), GetNickname(l.Login), l.Login))
                .ToList();

            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetGuestListManiaLinkPage(pageIndex.Value + 1, maxPageIndex + 1, playerListEntriesToShow), 0, false);

            Dictionary<byte, string> rowSettings = new Dictionary<byte, string>();
            byte rowIndex = 0;
            playerListEntriesToShow.ForEach(p => { rowSettings[rowIndex] = p.Login; rowIndex++; });
            GetAreaSettings(login, (byte)Area.GuestListArea).CustomData = rowSettings;
        }

        private string GetGuestListManiaLinkPage(uint currentPage, uint maxPage, IEnumerable<PlayerListEntry> logins)
        {
            string mainTemplateString = GuestListSettings.SinglePageTemplate;

            if (maxPage > 1)
            {
                if (currentPage == 1)
                    mainTemplateString = GuestListSettings.FirstPageTemplate;
                else
                    mainTemplateString = currentPage == maxPage ? GuestListSettings.LastPageTemplate : GuestListSettings.MiddlePageTemplate;
            }

            mainTemplateString = ReplaceMessagePlaceHolders(mainTemplateString, GuestListActions.GetReplaceParameters());

            XElement mainTemplate = XElement.Parse(FormatMessage(mainTemplateString, "ManiaLinkID", GUESTLIST_PANEL_ID, "CurrentPage", currentPage.ToString(CultureInfo.InvariantCulture), "MaxPage", maxPage.ToString(CultureInfo.InvariantCulture)));
            XElement entryPlaceHolder = mainTemplate.Descendants("PlayerPlaceHolder").First();
            double currentY = GuestListSettings.FirstEntryTopMargin;

            XElement lastInsertedNode = entryPlaceHolder;

            byte rowIndex = 0;

            foreach (PlayerListEntry playerListEntry in logins)
            {
                XElement currentElement = GetGuestListPlayerElement(playerListEntry, currentY, rowIndex);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= GuestListSettings.EntryHeight;
                rowIndex++;
            }

            entryPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetGuestListPlayerElement(PlayerListEntry playerEntry, double currentY, byte rowIndex)
        {
            return XElement.Parse
            (
                FormatMessage
                (
                    GuestListSettings.EntryTemplate,
                    "Y", currentY.ToString(CultureInfo.InvariantCulture),
                    "Position", playerEntry.Position.ToString(CultureInfo.InvariantCulture),
                    "Nickname", SecurityElement.Escape(playerEntry.Nickname ?? "[" + playerEntry.Login + "]"),
                    "Login", SecurityElement.Escape(playerEntry.Login),
                    "RemoveActionID", TMAction.CalculateActionID(ID, (byte)Area.GuestListArea, rowIndex, (byte)GuestListRowAction.RemovePlayer).ToString(CultureInfo.InvariantCulture)
                )
            );
        }

        #endregion

        #region Embedded Types

        private enum GuestListRowAction
        {
            RemovePlayer = 1,
        }

        #endregion
    }
}