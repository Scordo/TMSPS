﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;

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

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            RunCatchLog(() =>
            {
                if (e.IsServerMessage || e.Text.IsNullOrTimmedEmpty())
                    return;

                if (CheckForGetAllSpectatorsCommand(e))
                    return;

                if (CheckForGetMySpectatorsCommand(e))
                    return;

                if (CheckForKickAllSpectatorsCommand(e))
                    return;

                if (CheckForKickMySpectatorsCommand(e))
                    return;

            }, "Error in Callbacks_PlayerChat Method.", true);
        }

        private bool CheckForGetAllSpectatorsCommand(PlayerChatEventArgs e)
        {
            if (ServerCommand.Parse(e.Text).IsMainCommandAnyOf(CommandOrRight.GET_SPECTATORS1, CommandOrRight.GET_SPECTATORS2))
            {
                if (!LoginHasAnyRight(e.Login, true, CommandOrRight.GET_SPECTATORS1, CommandOrRight.GET_SPECTATORS2))
                    return true;

                List<string> spectators = Context.PlayerSettings.GetAsList(playerSettings => playerSettings.SpectatorStatus.IsSpectator)
                    .Select(playerSettings => StripTMColorsAndFormatting(playerSettings.NickName) + "[{[#HighlightStyle]}" + playerSettings.Login + "{[#MessageStyle]}]")
                    .ToList();

                if (spectators.Count > 0)
                    SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#HighlightStyle]}" + spectators.Count + " Spectator(s): {[#MessageStyle]}" + string.Join(", ", spectators.ToArray()));
                else
                    SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> Currently no spectators!");

                return true;
            }

            return false;
        }

        private bool CheckForGetMySpectatorsCommand(PlayerChatEventArgs e)
        {
            if (ServerCommand.Parse(e.Text).IsMainCommandAnyOf(CommandOrRight.GET_MY_SPECTATORS1, CommandOrRight.GET_MY_SPECTATORS2))
            {
                if (!LoginHasAnyRight(e.Login, true, CommandOrRight.GET_SPECTATORS1, CommandOrRight.GET_SPECTATORS2))
                    return true;

                List<string> spectators = Context.PlayerSettings.GetAsList(playerSettings => playerSettings.SpectatorStatus.IsSpectator && playerSettings.SpectatorStatus.CurrentPlayerTargetID == e.PlayerID)
                    .Select(playerSettings => StripTMColorsAndFormatting(playerSettings.NickName) + "[{[#HighlightStyle]}" + playerSettings.Login + "{[#MessageStyle]}]")
                    .ToList();

                if (spectators.Count > 0)
                    SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#HighlightStyle]}" + spectators.Count + " Spectator(s): {[#MessageStyle]}" + string.Join(", ", spectators.ToArray()));
                else
                    SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> Currently no one is spectating you!");

                return true;
            }


            return false;
        }

        private bool CheckForKickAllSpectatorsCommand(PlayerChatEventArgs e)
        {
            if (ServerCommand.Parse(e.Text).IsMainCommandAnyOf(CommandOrRight.KICK_SPECTATORS1, CommandOrRight.KICK_SPECTATORS2))
            {
                Context.CorePlugin.KickAllSpectators(e.Login);
                return true;
            }

            return false;
        }

        private bool CheckForKickMySpectatorsCommand(PlayerChatEventArgs e)
        {
            if (ServerCommand.Parse(e.Text).IsMainCommandAnyOf(CommandOrRight.KICK_MY_SPECTATORS1, CommandOrRight.KICK_MY_SPECTATORS2))
            {
                Context.CorePlugin.KickSpectatorsOf(e.Login, e.PlayerID);
                return true;
            }

            return false;
        }

        #endregion
    }
}