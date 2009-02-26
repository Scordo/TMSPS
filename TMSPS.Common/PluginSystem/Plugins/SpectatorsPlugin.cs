using System;
using System.Collections.Generic;
using System.Linq;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;

namespace TMSPS.Core.PluginSystem.Plugins
{
    public class SpectatorsPlugin : TMSPSPlugin
    {
        #region Constants

        public const string GET_SPECTATORS_RIGHT = "GetSpectatorsRight";
        public const string KICK_SPECTATORS_RIGHT = "KickSpectatorsRight";
        public const string GET_SPECTATORS_COMMAND1 = "GetSpectators";
        public const string GET_SPECTATORS_COMMAND2 = "Specs";

        public const string KICK_SPECTATORS_COMMAND1 = "KickSpectators";
        public const string KICK_SPECTATORS_COMMAND2 = "KickSpecs";

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
            get { return "Get Spectators Plugin"; }
        }

        public override string Description
        {
            get { return "Gives the ability to get a list of spectators"; }
        }

        public override string ShortName
        {
            get { return "Spectators"; }
        }

        #endregion

        #region Methods

        protected override void Init()
        {
            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            if (e.Erroneous)
            {
                Logger.Error(string.Format("[Callbacks_PlayerChat] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
                return;
            }

            RunCatchLog(() =>
            {
                if (e.IsServerMessage || e.Text.IsNullOrTimmedEmpty())
                    return;

                if (CheckForGetSpectatorsCommand(e))
                    return;

                if (CheckForKickSpectatorsCommand(e))
                    return;

            }, "Error in Callbacks_PlayerChat Method.", true);
        }

        private bool CheckForGetSpectatorsCommand(PlayerChatEventArgs e)
        {
            if (ServerCommand.Parse(e.Text).IsMainCommandAnyOf(GET_SPECTATORS_COMMAND1, GET_SPECTATORS_COMMAND2))
            {
                if (Context.Credentials.UserHasRight(e.Login, GET_SPECTATORS_RIGHT))
                {
                    List<PlayerInfo> players = GetPlayerList();

                    if (players == null)
                        return true;

                    List<string> spectators = players.Where(playerInfo => playerInfo.IsSpectator)
                        .Select(playerInfo => string.Format("{0} $z [{1}]$z", playerInfo.NickName, playerInfo.Login))
                        .ToList();

                    if (spectators.Count > 0)
                        Context.RPCClient.Methods.ChatSendToLogin(string.Format("{0} spectator(s): {1}", spectators.Count, string.Join(", ", spectators.ToArray())), e.Login);
                    else
                        Context.RPCClient.Methods.ChatSendToLogin("Currently no spectators!", e.Login);
                }
                else
                {
                    Context.RPCClient.Methods.ChatSendToLogin("You do not have permissions to execute this command!", e.Login);
                }

                return true;
            }


            return false;
        }

        private bool CheckForKickSpectatorsCommand(PlayerChatEventArgs e)
        {
            if (ServerCommand.Parse(e.Text).IsMainCommandAnyOf(KICK_SPECTATORS_COMMAND1, KICK_SPECTATORS_COMMAND2))
            {
                if (Context.Credentials.UserHasRight(e.Login, GET_SPECTATORS_RIGHT))
                {
                    List<PlayerInfo> players = GetPlayerList();

                    if (players == null)
                        return true;

                    List<PlayerInfo> logins = players.Where(playerInfo => playerInfo.IsSpectator).ToList();

                    foreach (PlayerInfo playerInfo in logins)
                    {
                        Context.RPCClient.Methods.Kick(playerInfo.Login, "Kicked for spectating without asking.");
                        Context.RPCClient.Methods.ChatSendServerMessage(string.Format("[SpectatorBot] {0} got kicked for spectating without asking.", playerInfo.NickName));
                    }

                    if (logins.Count == 0)
                        Context.RPCClient.Methods.ChatSendServerMessageToLogin("No one is spectating!", e.Login);
                    else
                        Context.RPCClient.Methods.ChatSendServerMessageToLogin(string.Format("Kicked {0} players for spectating without asking.", logins.Count), e.Login);
                }
                else
                {
                    Context.RPCClient.Methods.ChatSendServerMessageToLogin("You do not have permissions to execute this command.", e.Login);
                }

                return true;
            }

            return false;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
        }

        #endregion
    }
}