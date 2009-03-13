using System.Collections.Generic;
using System.Linq;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.ManiaLinking;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.AdminPlayer
{
    public class AdminPlayerPlugin : TMSPSPlugin
    {
        #region Constants

        public const string RESTART_TRACK_IMMEDIATELY_RIGHT = "RestartTrack";
        public const string NEXT_TRACK_RIGHT = "NextTrack";

        #endregion

        #region Members

        private readonly string _playerPanelID = "AdminPlayerPanelID"; //Guid.NewGuid().ToString("N");

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
                    HandleMainAreaActions(login, action);
                    break;
            }
        }

        private void HandleMainAreaActions(string login, TMAction areaAction)
        {
            MainAreaAction action = (MainAreaAction) areaAction.AreaActionID;

            switch (action)
            {
                case MainAreaAction.KickSpectators:
                    KickSpectators(login);
                    break;
                case MainAreaAction.RestartTrackImmediately:
                    RestartTrackImmediately(login);
                    break;
                case MainAreaAction.SwitchToNextTrack:
                    SwitchToNextMap(login);
                    break;
                case MainAreaAction.ShowPlayerListWithAdminAbilities:
                    ShowPlayerListWithAdminAbilities(login);
                    break;
            }
        }


        private void KickSpectators(string login)
        {
            if (!LoginHasAnyRight(login, true, SpectatorsPlugin.KICK_SPECTATORS_COMMAND1, SpectatorsPlugin.KICK_SPECTATORS_COMMAND2))
                return;

            List<PlayerInfo> players = GetPlayerList();

            if (players == null)
                return;

            List<PlayerInfo> logins = players.Where(playerInfo => playerInfo.IsSpectator).ToList();

            foreach (PlayerInfo playerInfo in logins)
            {
                Context.RPCClient.Methods.Kick(playerInfo.Login, "Kicked for spectating without asking.");
                SendFormattedMessage("{[#ServerStyle]}>> {[#HighlightStyle]}" + StripTMColorsAndFormatting(playerInfo.NickName) + " {[#MessageStyle]}got kicked for spectating without asking.");
            }

            if (logins.Count == 0)
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#MessageStyle]} No one is spectating!");
            else
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#MessageStyle]}Kicked {[#HighlightStyle]}" + logins.Count + "{[#MessageStyle]} player for spectating without asking.");
        }

        private void RestartTrackImmediately(string login)
        {
            if (!LoginHasAnyRight(login, true, RESTART_TRACK_IMMEDIATELY_RIGHT))
                return;

            Context.RPCClient.Methods.RestartChallenge();
        }

        private void SwitchToNextMap(string login)
        {
            if (!LoginHasAnyRight(login, true, RESTART_TRACK_IMMEDIATELY_RIGHT))
                return;
          
            Context.RPCClient.Methods.NextChallenge();
        }

        private void ShowPlayerListWithAdminAbilities(string login)
        {

        }

        private void Callbacks_PlayerConnect(object sender, Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            SendPlayerUIToLogin(e.Login);
        }

        private void SendToAllLogins()
        {
            List<PlayerInfo> players = GetPlayerList();
            if (players  == null)
                return;

            foreach (PlayerInfo player in players)
            {
                SendPlayerUIToLogin(player.Login);
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
                "ManiaLinkID", _playerPanelID,
                MainAreaAction.RestartTrackImmediately.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.RestartTrackImmediately).ToString(),
                MainAreaAction.KickSpectators.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.KickSpectators).ToString(),
                MainAreaAction.SwitchToNextTrack.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.SwitchToNextTrack).ToString(),
                MainAreaAction.ShowPlayerListWithAdminAbilities.ToString(), TMAction.CalculateActionID(ID, (int)Area.MainArea, (int)MainAreaAction.ShowPlayerListWithAdminAbilities).ToString()
            );

            return result;
        }

        private bool LoginHasAnyAdminPlayerRight(string login)
        {
            return LoginHasAnyRight(login, false,   RESTART_TRACK_IMMEDIATELY_RIGHT, 
                                                    NEXT_TRACK_RIGHT, 
                                                    SpectatorsPlugin.KICK_SPECTATORS_COMMAND1, 
                                                    SpectatorsPlugin.KICK_SPECTATORS_COMMAND2);
        }
        
        #endregion

        private enum Area
        {
            MainArea = 1,
            ManagePlayersArea = 2
        }

        private enum MainAreaAction
        {
            RestartTrackImmediately = 1,
            KickSpectators = 2,
            SwitchToNextTrack = 3,
            ShowPlayerListWithAdminAbilities = 4
        }
    }
}