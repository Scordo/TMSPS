using System.IO;
using TMSPS.Core.Common;
using TMSPS.Core.ManiaLinking;
using TMSPS.Core.PluginSystem.Configuration;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.AdminPlayer
{
    public partial class AdminPlayerPlugin : TMSPSPlugin
    {
        #region Constants

        private const string PLAYER_PANEL_ID = "AdminPlayerPanelID";

        #endregion

        #region Properties

        public override Version Version
        {
            get { return new Version("1.0.0.0"); }
        }

        public override string Author
        {
            get { return "Jens Hofmann"; }
        }

        public override string Name
        {
            get { return "AdminPlayerPlugin"; }
        }

        public override string Description
        {
            get { return "Shows a player whee admins can skip maps and so on.."; }
        }

        public override string ShortName
        {
            get { return "AdminPlayer"; }
        }


        #endregion

        #region Methods

        protected override void Init()
        {
            SendToAllLogins();

            GuestListSettings = PagedUIDialogSettingsBase<PagedUIDialogSettings>.ReadFromFile(Path.Combine(PluginDirectory, "GuestListUITemplate.xml"));
            GuestListActions = new PagedDialogActions(ID, (byte)Area.GuestListArea);

            IgnoreListSettings = PagedUIDialogSettingsBase<PagedUIDialogSettings>.ReadFromFile(Path.Combine(PluginDirectory, "IgnoreListUITemplate.xml"));
            IgnoreListActions = new PagedDialogActions(ID, (byte)Area.IgnoreListArea);

            BanListSettings = PagedUIDialogSettingsBase<PagedUIDialogSettings>.ReadFromFile(Path.Combine(PluginDirectory, "BanListUITemplate.xml"));
            BanListActions = new PagedDialogActions(ID, (byte)Area.BanListArea);

            BlackListSettings = PagedUIDialogSettingsBase<PagedUIDialogSettings>.ReadFromFile(Path.Combine(PluginDirectory, "BlackListUITemplate.xml"));
            BlackListActions = new PagedDialogActions(ID, (byte)Area.BlackListArea);

            LivePlayerSettings = LivePlayerUIDialogSettings.ReadFromFile(Path.Combine(PluginDirectory, "LivePlayerUITemplate.xml"));
            LivePlayerActions = new PagedDialogActions(ID, (byte)Area.LivePlayersArea);

            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
        }

        protected override void OnManiaLinkPageAnswer(string login, int playerID, TMAction action)
        {
            switch ((Area)action.AreaID)
            {
                case Area.MainArea:
                    HandleMainAreaActions(login, playerID, action);
                    break;
                case Area.GuestListArea:
                    HandleGuestListAreaActions(login, action);
                    break;
                case Area.IgnoreListArea:
                    HandleIgnoreListAreaActions(login, action);
                    break;
                case Area.BanListArea:
                    HandleBanListAreaActions(login, action);
                    break;
                case Area.BlackListArea:
                    HandleBlackListAreaActions(login, action);
                    break;
                case Area.LivePlayersArea:
                    HandleLivePlayersAreaActions(login, action);
                    break;
            }
        }

        private void HandleMainAreaActions(string login,  int playerID, TMAction areaAction)
        {
            MainAreaAction action = (MainAreaAction) areaAction.AreaActionID;

            switch (action)
            {
                case MainAreaAction.KickSpectators:
                    Context.CorePlugin.KickAllSpectators(login);
                    break;
                case MainAreaAction.RestartTrackImmediately:
                    RestartTrackImmediately(login);
                    break;
                case MainAreaAction.SwitchToNextTrack:
                    SwitchToNextMap(login);
                    break;
                case MainAreaAction.ShowLivePlayerList:
                    SendLivePlayerListPageToLogin(login, 0);
                    break;
                case MainAreaAction.ShowGuestList:
                    SendGuestListPageToLogin(login, 0);
                    break;
                case MainAreaAction.ShowIgnoreList:
                    SendIgnoreListPageToLogin(login, 0);
                    break;
                case MainAreaAction.ShowBanList:
                    SendBanListPageToLogin(login, 0);
                    break;
                case MainAreaAction.ShowBlackList:
                    SendBlackListPageToLogin(login, 0);
                    break;
                case MainAreaAction.KickMySpectators:
                    Context.CorePlugin.KickSpectatorsOf(login, playerID);
                    break;
            }
        }

        private void RestartTrackImmediately(string login)
        {
            if (!LoginHasAnyRight(login, true, CommandOrRight.RESTART_TRACK_IMMEDIATELY))
                return;

            Context.RPCClient.Methods.RestartChallenge();
        }

        private void SwitchToNextMap(string login)
        {
            if (!LoginHasAnyRight(login, true, CommandOrRight.NEXT_TRACK))
                return;
          
            Context.RPCClient.Methods.NextChallenge();
        }

        private void Callbacks_PlayerConnect(object sender, Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            SendPlayerUIToLogin(e.Login);
        }

        private void SendToAllLogins()
        {
            foreach (PlayerSettings playerSetting in Context.PlayerSettings.GetAllAsList())
            {
                SendPlayerUIToLogin(playerSetting.Login);
            }
        }

        private void SendPlayerUIToLogin(string login)
        {
            if (LoginHasAnyAdminPlayerRight(login))
                Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetAdminPlayerManiaLinkPage(), 0, false);
        }

        private string GetAdminPlayerManiaLinkPage()
        {
            
            string result = ReplaceMessagePlaceHolders
            (
                UITemplates.PlayerTemplate,
                "ManiaLinkID", PLAYER_PANEL_ID,
                MainAreaAction.RestartTrackImmediately.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.RestartTrackImmediately).ToString(),
                MainAreaAction.KickSpectators.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.KickSpectators).ToString(),
                MainAreaAction.SwitchToNextTrack.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.SwitchToNextTrack).ToString(),
                MainAreaAction.ShowLivePlayerList.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.ShowLivePlayerList).ToString(),
                MainAreaAction.ShowGuestList.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.ShowGuestList).ToString(),
                MainAreaAction.ShowIgnoreList.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.ShowIgnoreList).ToString(),
                MainAreaAction.ShowBanList.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.ShowBanList).ToString(),
                MainAreaAction.ShowBlackList.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.ShowBlackList).ToString(),
                MainAreaAction.KickMySpectators.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.KickMySpectators).ToString()
            );

            return result;
        }

        private bool LoginHasAnyAdminPlayerRight(string login)
        {
            return LoginHasAnyRight(login, false,   CommandOrRight.RESTART_TRACK_IMMEDIATELY,
                                                    CommandOrRight.NEXT_TRACK, 
                                                    CommandOrRight.KICK_SPECTATORS1,
                                                    CommandOrRight.KICK_SPECTATORS2,
                                                    CommandOrRight.ADD_GUEST,
                                                    CommandOrRight.KICK,
                                                    CommandOrRight.BAN,
                                                    CommandOrRight.BLACKLIST,
                                                    CommandOrRight.IGNORE,
                                                    CommandOrRight.KICK_MY_SPECTATORS1,
                                                    CommandOrRight.KICK_MY_SPECTATORS2);
        }
        
        #endregion

        #region Embedded Types

        private enum Area
        {
            MainArea = 1,
            LivePlayersArea = 2,
            GuestListArea = 3,
            IgnoreListArea = 4,
            BanListArea = 5,
            BlackListArea = 6
        }

        private enum MainAreaAction
        {
            RestartTrackImmediately = 1,
            KickSpectators = 2,
            SwitchToNextTrack = 3,
            ShowLivePlayerList = 4,
            ShowGuestList = 5,
            ShowIgnoreList = 6,
            ShowBanList = 7,
            ShowBlackList = 8,
            KickMySpectators = 9
        }


        internal class PlayerListEntry
        {
            public ushort Position { get; set; }
            public string Nickname { get; set; }
            public string Login { get; set; }

            public PlayerListEntry()
            {
            }

            public PlayerListEntry(ushort position, string nickname, string login)
            {
                Position = position;
                Nickname = nickname;
                Login = login;
            }
        }

        #endregion
    }
}