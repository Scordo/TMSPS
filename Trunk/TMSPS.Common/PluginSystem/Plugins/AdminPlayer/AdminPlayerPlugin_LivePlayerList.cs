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

namespace TMSPS.Core.PluginSystem.Plugins.AdminPlayer
{
    public partial class AdminPlayerPlugin
    {
        #region Constants

        private const string LIVEPLAYERLIST_PANEL_ID = "AdminLivePlayerListPanelID";

        #endregion

        #region Properties

        private LivePlayerUIDialogSettings LivePlayerSettings { get; set; }
        private PagedDialogActions LivePlayerActions { get; set; }

        #endregion

        private void HandleLivePlayersAreaActions(string login, TMAction areaAction)
        {
            if (areaAction.IsAreaAction)
            {
                PagedDialogActions.DefaultDialogAction action = (PagedDialogActions.DefaultDialogAction)areaAction.AreaActionID;

                switch (action)
                {
                    case PagedDialogActions.DefaultDialogAction.CloseDialog:
                        GetPluginSettings(login).AreaSettings.Reset((byte)Area.LivePlayersArea);
                        SendEmptyManiaLinkPageToLogin(login, LIVEPLAYERLIST_PANEL_ID);
                        break;
                    case PagedDialogActions.DefaultDialogAction.FirstPage:
                        SendLivePlayerListPageToLogin(login, 0);
                        break;
                    case PagedDialogActions.DefaultDialogAction.PrevPage:
                        ushort prevPageIndex = Convert.ToUInt16(Math.Max(0, GetAreaSettings(login, (byte)Area.LivePlayersArea).CurrentDialogPageIndex - 1));
                        SendLivePlayerListPageToLogin(login, prevPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.NextPage:
                        ushort nextPageIndex = Convert.ToUInt16(GetAreaSettings(login, (byte)Area.LivePlayersArea).CurrentDialogPageIndex + 1);
                        SendLivePlayerListPageToLogin(login, nextPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.LastPage:
                        SendLivePlayerListPageToLogin(login, null);
                        break;
                }
            }
            else
            {
                LivePlayerListRowAction action = (LivePlayerListRowAction)areaAction.RowActionID;

                switch (action)
                {
                    case LivePlayerListRowAction.KickPlayer:
                        LivePlayerList_KickPlayer(login, areaAction.RowIndex);
                        break;
                    case LivePlayerListRowAction.IgnorePlayer:
                        LivePlayerList_IgnorePlayer(login, areaAction.RowIndex);
                        break;
                    case LivePlayerListRowAction.UnIgnorePlayer:
                        LivePlayerList_UnIgnorePlayer(login, areaAction.RowIndex);
                        break;
                    case LivePlayerListRowAction.BanPlayer:
                        LivePlayerList_AddBannedPlayer(login, areaAction.RowIndex);
                        break;
                    case LivePlayerListRowAction.UnBanPlayer:
                        LivePlayerList_RemoveBannedPlayer(login, areaAction.RowIndex);
                        break;
                    case LivePlayerListRowAction.BlackListPlayer:
                        LivePlayerList_AddBlackListPlayer(login, areaAction.RowIndex);
                        break;
                    case LivePlayerListRowAction.UnBlackListPlayer:
                        LivePlayerList_RemoveBlackListPlayer(login, areaAction.RowIndex);
                        break;
                    case LivePlayerListRowAction.AddGuest:
                        LivePlayerList_AddGuestPlayer(login, areaAction.RowIndex);
                        break;
                    case LivePlayerListRowAction.RemoveGuest:
                        LivePlayerList_RemoveGuestPlayer(login, areaAction.RowIndex);
                        break;
                    case LivePlayerListRowAction.ForceSpectator:
                        LivePlayerList_ForceSpectatorPlayer(login, areaAction.RowIndex);
                        break;
                }
            }
        }

        private void LivePlayerList_ForceSpectatorPlayer(string login, byte rowIndex)
        {
            Context.CorePlugin.ForceSpectatorLogin(login, GetLoginByRowIndex(login, rowIndex));
            ResendCurrentLivePlayerListPage(login);
        }

        private void LivePlayerList_KickPlayer(string login, byte rowIndex)
        {
            Context.CorePlugin.KickLogin(login, GetLoginByRowIndex(login, rowIndex));
            ResendCurrentLivePlayerListPage(login);
        }

        private void LivePlayerList_IgnorePlayer(string login, byte rowIndex)
        {
            Context.CorePlugin.IgnoreLogin(login, GetLoginByRowIndex(login, rowIndex));
            ResendCurrentLivePlayerListPage(login);
        }

        private void LivePlayerList_UnIgnorePlayer(string login, byte rowIndex)
        {
            Context.CorePlugin.UnIgnoreLogin(login, GetLoginByRowIndex(login, rowIndex));
            ResendCurrentLivePlayerListPage(login);
        }

        private void LivePlayerList_AddBannedPlayer(string login, byte rowIndex)
        {
            Context.CorePlugin.BanLogin(login, GetLoginByRowIndex(login, rowIndex));
            ResendCurrentLivePlayerListPage(login);
        }

        private void LivePlayerList_RemoveBannedPlayer(string login, byte rowIndex)
        {
            Context.CorePlugin.UnBanLogin(login, GetLoginByRowIndex(login, rowIndex));
            ResendCurrentLivePlayerListPage(login);
        }

        private void LivePlayerList_AddBlackListPlayer(string login, byte rowIndex)
        {
            Context.CorePlugin.AddBlackListLogin(login, GetLoginByRowIndex(login, rowIndex));
            ResendCurrentLivePlayerListPage(login);
        }

        private void LivePlayerList_RemoveBlackListPlayer(string login, byte rowIndex)
        {
            Context.CorePlugin.RemoveBlackListLogin(login, GetLoginByRowIndex(login, rowIndex));
            ResendCurrentLivePlayerListPage(login);
        }

        private void LivePlayerList_AddGuestPlayer(string login, byte rowIndex)
        {
            Context.CorePlugin.AddGuestLogin(login, GetLoginByRowIndex(login, rowIndex));
            ResendCurrentLivePlayerListPage(login);
        }

        private void LivePlayerList_RemoveGuestPlayer(string login, byte rowIndex)
        {
            Context.CorePlugin.RemoveGuestLogin(login, GetLoginByRowIndex(login, rowIndex));
            ResendCurrentLivePlayerListPage(login);
        }

        private void ResendCurrentLivePlayerListPage(string login)
        {
            SendLivePlayerListPageToLogin(login, GetAreaSettings(login, (byte)Area.LivePlayersArea).CurrentDialogPageIndex);
        }

        private void SendLivePlayerListPageToLogin(string login, uint? pageIndex)
        {
            if (!LoginHasAnyRight(login, true, Command.KICK, Command.BAN, Command.BLACKLIST, Command.IGNORE, Command.ADD_GUEST))
                return;

            HashSet<string> ignoreList = GetIgnoreList();
            HashSet<string> banList = GetBanList();
            HashSet<string> blackList = GetBlackList();
            HashSet<string> guestList = GetGuestList();
            List<PlayerInfo> playerList;
            HashSet<string> spectatorList = GetSpectatorList(out playerList);


            uint maxPageIndex = Convert.ToUInt32(Math.Max(0, Math.Ceiling((double)playerList.Count / LivePlayerSettings.MaxEntriesPerPage) - 1));

            if (!pageIndex.HasValue)
                pageIndex = maxPageIndex;

            pageIndex = Convert.ToUInt16(Math.Min(Math.Max(0, (int)pageIndex), maxPageIndex));
            GetAreaSettings(login, (byte)Area.LivePlayersArea).CurrentDialogPageIndex = (ushort)pageIndex;

            int entriesToSkip = Convert.ToInt32(pageIndex * LivePlayerSettings.MaxEntriesPerPage);
            int startPosition = entriesToSkip + 1;

            List<PlayerListEntry> playerListEntriesToShow = playerList.Skip(entriesToSkip)
                .Take(Convert.ToInt32(LivePlayerSettings.MaxEntriesPerPage))
                .Select((l, i) => new PlayerListEntry((ushort)(startPosition + i), l.NickName, l.Login))
                .ToList();

            string pageContent = GetLivePlayerListManiaLinkPage(pageIndex.Value + 1, maxPageIndex + 1, playerListEntriesToShow, ignoreList, banList, blackList, guestList, spectatorList);
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, pageContent, 0, false);

            Dictionary<byte, string> rowSettings = new Dictionary<byte, string>();
            byte rowIndex = 0;
            playerListEntriesToShow.ForEach(p => { rowSettings[rowIndex] = p.Login; rowIndex++; });

            GetAreaSettings(login, (byte)Area.LivePlayersArea).CustomData = rowSettings;
        }

       

        private string GetLivePlayerListManiaLinkPage(uint currentPage, uint maxPage, IEnumerable<PlayerListEntry> logins, ICollection<string> ignoreList, ICollection<string> banList, ICollection<string> blackList, ICollection<string> guestList, ICollection<string> spectatorList)
        {
            string mainTemplateString = LivePlayerSettings.SinglePageTemplate;

            if (maxPage > 1)
            {
                if (currentPage == 1)
                    mainTemplateString = LivePlayerSettings.FirstPageTemplate;
                else
                    mainTemplateString = currentPage == maxPage ? LivePlayerSettings.LastPageTemplate : LivePlayerSettings.MiddlePageTemplate;
            }

            mainTemplateString = ReplaceMessagePlaceHolders(mainTemplateString, LivePlayerActions.GetReplaceParameters());

            XElement mainTemplate = XElement.Parse(FormatMessage(mainTemplateString, "ManiaLinkID", LIVEPLAYERLIST_PANEL_ID, "CurrentPage", currentPage.ToString(CultureInfo.InvariantCulture), "MaxPage", maxPage.ToString(CultureInfo.InvariantCulture)));
            XElement entryPlaceHolder = mainTemplate.Descendants("PlayerPlaceHolder").First();
            double currentY = LivePlayerSettings.FirstEntryTopMargin;

            XElement lastInsertedNode = entryPlaceHolder;

            byte rowIndex = 0;

            foreach (PlayerListEntry playerListEntry in logins)
            {
                string login = playerListEntry.Login;
                IList<LivePlayerListRowAction?> rowActions = new []
                {
                    LivePlayerListRowAction.WarnPlayer,
                    ignoreList.Contains(login) ? LivePlayerListRowAction.UnIgnorePlayer : LivePlayerListRowAction.IgnorePlayer, 
                    LivePlayerListRowAction.KickPlayer, 
                    banList.Contains(login) ? LivePlayerListRowAction.UnBanPlayer : LivePlayerListRowAction.BanPlayer, 
                    blackList.Contains(login) ? LivePlayerListRowAction.UnBlackListPlayer : LivePlayerListRowAction.BlackListPlayer, 
                    guestList.Contains(login) ? LivePlayerListRowAction.RemoveGuest : LivePlayerListRowAction.AddGuest, 
                    spectatorList.Contains(login) ? null : (LivePlayerListRowAction?)LivePlayerListRowAction.ForceSpectator, 
                };

                XElement currentElement = GetLivePlayerListPlayerElement(playerListEntry, currentY, rowIndex, rowActions);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= LivePlayerSettings.EntryHeight;
                rowIndex++;
            }

            entryPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetLivePlayerListPlayerElement(PlayerListEntry playerEntry, double currentY, byte rowIndex, IList<LivePlayerListRowAction?> actions)
        {
            const byte livePlayerAreaID = (byte) Area.LivePlayersArea;

            return XElement.Parse
            (
                FormatMessage
                (
                    LivePlayerSettings.EntryTemplate,
                    "Y", currentY.ToString(CultureInfo.InvariantCulture),
                    "Position", playerEntry.Position.ToString(CultureInfo.InvariantCulture),
                    "Nickname", SecurityElement.Escape(playerEntry.Nickname ?? "[" + playerEntry.Login + "]"),
                    "Login", SecurityElement.Escape(playerEntry.Login),
                    
                    "IgnoreText", actions[1] == LivePlayerListRowAction.IgnorePlayer ? LivePlayerSettings.IgnoreText : LivePlayerSettings.UnIgnoreText,
                    "BanText", actions[3] == LivePlayerListRowAction.BanPlayer ? LivePlayerSettings.BanText : LivePlayerSettings.UnBanText,
                    "BlackListText", actions[4] == LivePlayerListRowAction.BlackListPlayer ? LivePlayerSettings.AddToBlackListText : LivePlayerSettings.RemoveFromBlackListText,
                    "GuestText", actions[5] == LivePlayerListRowAction.AddGuest ? LivePlayerSettings.AddGuestText : LivePlayerSettings.RemoveGuestText,
                    "SpectatorText", actions[6] == LivePlayerListRowAction.ForceSpectator ? LivePlayerSettings.ForceSpectatorText : LivePlayerSettings.SpectatorText,

                    "WarnAction", TMAction.CalculateActionID(ID, livePlayerAreaID, rowIndex, (byte)actions[0]).ToString(CultureInfo.InvariantCulture),
                    "IgnoreAction", TMAction.CalculateActionID(ID, livePlayerAreaID, rowIndex, (byte)actions[1]).ToString(CultureInfo.InvariantCulture),
                    "KickAction", TMAction.CalculateActionID(ID, livePlayerAreaID, rowIndex, (byte)actions[2]).ToString(CultureInfo.InvariantCulture),
                    "BanAction", TMAction.CalculateActionID(ID, livePlayerAreaID, rowIndex, (byte)actions[3]).ToString(CultureInfo.InvariantCulture),
                    "BlackListAction", TMAction.CalculateActionID(ID, livePlayerAreaID, rowIndex, (byte)actions[4]).ToString(CultureInfo.InvariantCulture),
                    "GuestAction", TMAction.CalculateActionID(ID, livePlayerAreaID, rowIndex, (byte)actions[5]).ToString(CultureInfo.InvariantCulture),
                    "SpectatorAction", actions[6] == null ? "-" : TMAction.CalculateActionID(ID, livePlayerAreaID, rowIndex, (byte)actions[6]).ToString(CultureInfo.InvariantCulture)
                )
            );
        }

        private HashSet<string> GetIgnoreList()
        {
            GenericListResponse<LoginResponse> ignoreListResponse = Context.RPCClient.Methods.GetIgnoreList(1000, 0);
            HashSet<string> ignoreList = new HashSet<string>();

            if (!ignoreListResponse.Erroneous)
                ignoreList = new HashSet<string>(ignoreListResponse.Value.ConvertAll(x => x.Login));

            return ignoreList;
        }

        private HashSet<string> GetBanList()
        {
            GenericListResponse<BanEntry> banListResponse = Context.RPCClient.Methods.GetBanList(1000, 0);
            HashSet<string> banList = new HashSet<string>();

            if (!banListResponse.Erroneous)
                banList = new HashSet<string>(banListResponse.Value.ConvertAll(x => x.Login));

            return banList;
        }

        private HashSet<string> GetBlackList()
        {
            GenericListResponse<LoginResponse> blackListResponse = Context.RPCClient.Methods.GetBlackList(1000, 0);
            HashSet<string> blackList = new HashSet<string>();

            if (!blackListResponse.Erroneous)
                blackList = new HashSet<string>(blackListResponse.Value.ConvertAll(x => x.Login));

            return blackList;
        }

        private HashSet<string> GetGuestList()
        {
            GenericListResponse<LoginResponse> guestListResponse = Context.RPCClient.Methods.GetGuestList(1000, 0);
            HashSet<string> guestList = new HashSet<string>();

            if (!guestListResponse.Erroneous)
                guestList = new HashSet<string>(guestListResponse.Value.ConvertAll(x => x.Login));

            return guestList;
        }

        private HashSet<string> GetSpectatorList(out List<PlayerInfo> playerList)
        {
            GenericListResponse<PlayerInfo> playerListResponse = Context.RPCClient.Methods.GetPlayerList(1000, 0);
            HashSet<string> guestList = new HashSet<string>();
            playerList = new List<PlayerInfo>();


            if (!playerListResponse.Erroneous)
            {
                playerList = playerListResponse.Value;
                playerList.Sort((x, y) => x.PlayerId - y.PlayerId);
                guestList = new HashSet<string>(playerList.Where(x => x.IsSpectator).Select(x => x.Login).ToList());
            }

            return guestList;
        }

        private string GetLoginByRowIndex(string login, byte rowIndex)
        {
            Dictionary<byte, string> visibleLogins = (Dictionary<byte, string>)GetAreaSettings(login, (byte)Area.LivePlayersArea).CustomData;

            if (visibleLogins == null || !visibleLogins.ContainsKey(rowIndex))
                return null;

            return visibleLogins[rowIndex];
        }


        #region Embedded Types

        private enum LivePlayerListRowAction
        {
            WarnPlayer = 1,
            IgnorePlayer = 2,
            UnIgnorePlayer = 3,
            KickPlayer = 4,
            BanPlayer = 5,
            UnBanPlayer = 6,
            BlackListPlayer = 7,
            UnBlackListPlayer = 8,
            AddGuest = 9,
            RemoveGuest = 10,
            ForceSpectator = 11,
        }

        #endregion
    }
}
