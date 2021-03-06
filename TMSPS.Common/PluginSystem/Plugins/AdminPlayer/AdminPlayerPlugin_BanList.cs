﻿using System;
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

        private const string BANLIST_PANEL_ID = "AdminPlayerBanListPanelID";

        #endregion

        #region Properties

        private PagedUIDialogSettingsBase<PagedUIDialogSettings> BanListSettings { get; set; }
        private PagedDialogActions BanListActions { get; set; }

        #endregion

        #region Methods

        private void HandleBanListAreaActions(string login, TMAction areaAction)
        {
            if (areaAction.IsAreaAction)
            {
                PagedDialogActions.DefaultDialogAction action = (PagedDialogActions.DefaultDialogAction)areaAction.AreaActionID;

                switch (action)
                {
                    case PagedDialogActions.DefaultDialogAction.CloseDialog:
                        GetPluginSettings(login).AreaSettings.Reset((byte)Area.BanListArea);
                        SendEmptyManiaLinkPageToLogin(login, BANLIST_PANEL_ID);
                        break;
                    case PagedDialogActions.DefaultDialogAction.FirstPage:
                        SendBanListPageToLogin(login, 0);
                        break;
                    case PagedDialogActions.DefaultDialogAction.PrevPage:
                        ushort prevPageIndex = Convert.ToUInt16(Math.Max(0, GetAreaSettings(login, (byte)Area.BanListArea).CurrentDialogPageIndex - 1));
                        SendBanListPageToLogin(login, prevPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.NextPage:
                        ushort nextPageIndex = Convert.ToUInt16(GetAreaSettings(login, (byte)Area.BanListArea).CurrentDialogPageIndex + 1);
                        SendBanListPageToLogin(login, nextPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.LastPage:
                        SendBanListPageToLogin(login, null);
                        break;
                }
            }
            else
            {
                BanListRowAction action = (BanListRowAction)areaAction.RowActionID;

                switch (action)
                {
                    case BanListRowAction.RemovePlayer:
                        RemoveBanListPlayer(login, areaAction.RowIndex);
                        break;
                }
            }
        }

        private void RemoveBanListPlayer(string login, byte rowIndex)
        {
            Dictionary<byte, string> visibleLogins = (Dictionary<byte, string>)GetAreaSettings(login, (byte)Area.BanListArea).CustomData;

            if (visibleLogins == null || !visibleLogins.ContainsKey(rowIndex))
                return;

            string loginToRemove = visibleLogins[rowIndex];
            Context.CorePlugin.UnBanLogin(login, loginToRemove);
            SendBanListPageToLogin(login, GetAreaSettings(login, (byte)Area.BanListArea).CurrentDialogPageIndex);
        }

        private void SendBanListPageToLogin(string login, uint? pageIndex)
        {
            if (!LoginHasRight(login, true, Command.Ban))
                return;

            const int MAX_BANLIST_SIZE = 1000;

            GenericListResponse<BanEntry> BanListResponse = Context.RPCClient.Methods.GetBanList(MAX_BANLIST_SIZE, 0);

            if (BanListResponse.Erroneous)
            {
                Logger.Error("Error retrieving BanList: " + BanListResponse.Fault.FaultMessage);
                return;
            }

            List<BanEntry> BanList = BanListResponse.Value;

            uint maxPageIndex = Convert.ToUInt32(Math.Max(0, Math.Ceiling((double)BanList.Count / BanListSettings.MaxEntriesPerPage) - 1));

            if (!pageIndex.HasValue)
                pageIndex = maxPageIndex;

            pageIndex = Convert.ToUInt16(Math.Min(Math.Max(0, (int)pageIndex), maxPageIndex));
            GetAreaSettings(login, (byte)Area.BanListArea).CurrentDialogPageIndex = (ushort)pageIndex;

            int entriesToSkip = Convert.ToInt32(pageIndex * BanListSettings.MaxEntriesPerPage);
            int startPosition = entriesToSkip + 1;

            List<PlayerListEntry> playerListEntriesToShow = BanList.Skip(entriesToSkip)
                .Take(Convert.ToInt32(BanListSettings.MaxEntriesPerPage))
                .Select((l, i) => new PlayerListEntry((ushort)(startPosition + i), GetNickname(l.Login), l.Login))
                .ToList();

            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetBanListManiaLinkPage(pageIndex.Value + 1, maxPageIndex + 1, playerListEntriesToShow), 0, false);

            Dictionary<byte, string> rowSettings = new Dictionary<byte, string>();
            byte rowIndex = 0;
            playerListEntriesToShow.ForEach(p => { rowSettings[rowIndex] = p.Login; rowIndex++; });

            GetAreaSettings(login, (byte)Area.BanListArea).CustomData = rowSettings;
        }

        private string GetBanListManiaLinkPage(uint currentPage, uint maxPage, IEnumerable<PlayerListEntry> logins)
        {
            string mainTemplateString = BanListSettings.SinglePageTemplate;

            if (maxPage > 1)
            {
                if (currentPage == 1)
                    mainTemplateString = BanListSettings.FirstPageTemplate;
                else
                    mainTemplateString = currentPage == maxPage ? BanListSettings.LastPageTemplate : BanListSettings.MiddlePageTemplate;
            }

            mainTemplateString = ReplaceMessagePlaceHolders(mainTemplateString, BanListActions.GetReplaceParameters());

            XElement mainTemplate = XElement.Parse(FormatMessage(mainTemplateString, "ManiaLinkID", BANLIST_PANEL_ID, "CurrentPage", currentPage.ToString(CultureInfo.InvariantCulture), "MaxPage", maxPage.ToString(CultureInfo.InvariantCulture)));
            XElement entryPlaceHolder = mainTemplate.Descendants("PlayerPlaceHolder").First();
            double currentY = BanListSettings.FirstEntryTopMargin;

            XElement lastInsertedNode = entryPlaceHolder;

            byte rowIndex = 0;

            foreach (PlayerListEntry playerListEntry in logins)
            {
                XElement currentElement = GetBanListPlayerElement(playerListEntry, currentY, rowIndex);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= BanListSettings.EntryHeight;
                rowIndex++;
            }

            entryPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetBanListPlayerElement(PlayerListEntry playerEntry, double currentY, byte rowIndex)
        {
            return XElement.Parse
            (
                FormatMessage
                (
                    BanListSettings.EntryTemplate,
                    "Y", currentY.ToString(CultureInfo.InvariantCulture),
                    "Position", playerEntry.Position.ToString(CultureInfo.InvariantCulture),
                    "Nickname", SecurityElement.Escape(playerEntry.Nickname ?? "[" + playerEntry.Login + "]"),
                    "Login", SecurityElement.Escape(playerEntry.Login),
                    "RemoveActionID", TMAction.CalculateActionID(ID, (byte)Area.BanListArea, rowIndex, (byte)BanListRowAction.RemovePlayer).ToString(CultureInfo.InvariantCulture)
                )
            );
        }

        #endregion

        #region Embedded Types

        private enum BanListRowAction
        {
            RemovePlayer = 1,
        }

        #endregion
    }
}