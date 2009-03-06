using System.Collections.Generic;
using System.Linq;
using TMSPS.Core.Communication.ProxyTypes;
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
            Context.RPCClient.Callbacks.PlayerManialinkPageAnswer += Callbacks_PlayerManialinkPageAnswer;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerManialinkPageAnswer -= Callbacks_PlayerManialinkPageAnswer;
        }

        private void Callbacks_PlayerManialinkPageAnswer(object sender, Communication.EventArguments.Callbacks.PlayerManialinkPageAnswerEventArgs e)
        {
            AdminPlayerAnswers playerAction = (AdminPlayerAnswers) e.Answer;

            switch (playerAction)
            {
                case AdminPlayerAnswers.KickSpectators:
                    KickSpectators(e.Login);
                    break;
                case AdminPlayerAnswers.RestartTrackImmediately:
                    RestartTrackImmediately(e.Login);
                    break;
                case AdminPlayerAnswers.SwitchToNextTrack:
                    SwitchToNextMap(e.Login);
                    break;
                case AdminPlayerAnswers.ShowPlayerListWithAdminAbilities:
                    ShowPlayerListWithAdminAbilities();
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

        private void ShowPlayerListWithAdminAbilities()
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
                Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, UITemplates.PlayerTemplate.Replace("{[ManiaLinkID]}", _playerPanelID), 0, false);
        }

        private bool LoginHasAnyAdminPlayerRight(string login)
        {
            return LoginHasAnyRight(login, false,   RESTART_TRACK_IMMEDIATELY_RIGHT, 
                                                    NEXT_TRACK_RIGHT, 
                                                    SpectatorsPlugin.KICK_SPECTATORS_COMMAND1, 
                                                    SpectatorsPlugin.KICK_SPECTATORS_COMMAND2);
        }
        
        #endregion

        private enum AdminPlayerAnswers
        {
            RestartTrackImmediately = 20101,
            KickSpectators = 20102,
            SwitchToNextTrack = 20103,
            ShowPlayerListWithAdminAbilities = 20104
        }
    }
}