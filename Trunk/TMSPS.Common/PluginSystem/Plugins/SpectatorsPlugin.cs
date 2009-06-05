using System;
using System.Collections.Generic;
using System.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;

namespace TMSPS.Core.PluginSystem.Plugins
{
    public class SpectatorsPlugin : TMSPSPlugin
    {
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
            if (ServerCommand.Parse(e.Text).IsMainCommandAnyOf(Command.GET_SPECTATORS1, Command.GET_SPECTATORS2))
            {
                if (Context.Credentials.UserHasAnyRight(e.Login, Command.GET_SPECTATORS1, Command.GET_SPECTATORS2))
                {
                    List<PlayerInfo> players = GetPlayerList();

                    if (players == null)
                        return true;

                    List<string> spectators = players.Where(playerInfo => playerInfo.IsSpectator)
                        .Select(playerInfo => StripTMColorsAndFormatting(playerInfo.NickName) + "[{[#HighlightStyle]}" + playerInfo.Login +"{[#MessageStyle]}]")
                        .ToList();

                    if (spectators.Count > 0)
                        SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#HighlightStyle]}" + spectators.Count + " Spectator(s): {[#MessageStyle]}" + string.Join(", ", spectators.ToArray()));
                    else
                        SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> Currently no spectators!");
                }
                else
                {
                    SendNoPermissionMessagetoLogin(e.Login);
                }

                return true;
            }


            return false;
        }

        private bool CheckForKickSpectatorsCommand(PlayerChatEventArgs e)
        {
            if (ServerCommand.Parse(e.Text).IsMainCommandAnyOf(Command.KICK_SPECTATORS1, Command.KICK_SPECTATORS2))
            {
                if (Context.Credentials.UserHasAnyRight(e.Login, Command.KICK_SPECTATORS1, Command.KICK_SPECTATORS2))
                {
                    List<PlayerInfo> players = GetPlayerList();

                    if (players == null)
                        return true;

                    List<PlayerInfo> logins = players.Where(playerInfo => playerInfo.IsSpectator).ToList();

                    foreach (PlayerInfo playerInfo in logins)
                    {
                        Context.RPCClient.Methods.Kick(playerInfo.Login, "Kicked for spectating without asking.");
                        SendFormattedMessage("{[#ServerStyle]}>> {[#HighlightStyle]}" + StripTMColorsAndFormatting(playerInfo.NickName) + " {[#MessageStyle]}got kicked for spectating without asking.");
                    }

                    if (logins.Count == 0)
                        SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#MessageStyle]} No one is spectating!");
                    else
                        SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#MessageStyle]}Kicked {[#HighlightStyle]}" + logins.Count + "{[#MessageStyle]} player for spectating without asking.");
                }
                else
                {
                    SendNoPermissionMessagetoLogin(e.Login);
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