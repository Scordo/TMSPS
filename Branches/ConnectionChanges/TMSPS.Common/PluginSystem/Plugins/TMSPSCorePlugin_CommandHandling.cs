﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.ManiaLinking;
using TMSPS.Core.PluginSystem.Configuration;
using System.Linq;

namespace TMSPS.Core.PluginSystem.Plugins
{
    public partial class TMSPSCorePlugin
    {
        private PagedDialogActions HelpListActions { get; set; }
        private const string HELP_LIST_PANEL_ID = "HelpListPanelID";


        private void HandleCommand(string login, ServerCommand command)
        {
            if (command.Is(Command.Kick))
            {
                HandleKickCommand(login, command);
                return;
            }

            if (command.Is(Command.Warn))
            {
                HandleWarnCommand(login, command);
                return;
            }

            if (command.Is(Command.Ban))
            {
                HandleBanCommand(login, command);
                return;
            }

            if (command.Is(Command.Unban))
            {
                HandleUnBanCommand(login, command);
                return;
            }

            if (command.Is(Command.Blacklist))
            {
                HandleBlackListCommand(login, command);
                return;
            }

            if (command.Is(Command.Unblacklist))
            {
                HandleUnBlackListCommand(login, command);
                return;
            }

            if (command.Is(Command.Ignore))
            {
                HandleIgnoreCommand(login, command);
                return;
            }

            if (command.Is(Command.Unignore))
            {
                HandleUnIgnoreCommand(login, command);
                return;
            }

            if (command.Is(Command.AddGuest))
            {
                HandleAddGuestCommand(login, command);
                return;
            }

            if (command.Is(Command.RemoveGuest))
            {
                HandleRemoveGuestCommand(login, command);
                return;
            }

            if (command.Is(Command.ForceSpectator))
            {
                HandleForceSpectatorCommand(login, command);
                return;
            }

            if (command.Is(Command.WriteTrackList))
            {
                HandleWriteTrackListCommand(login, command);
                return;
            }

            if (command.Is(Command.ReadTrackList))
            {
                HandleReadTrackListCommand(login, command);
                return;
            }

            if (command.Is(Command.RemoveCurrentTrack))
            {
                HandleRemoveTrackCommand(login);
                return;
            }

            if (command.Is(Command.ReadCredentials))
            {
                HandleReadCredentialsCommand(login);
                return;
            }

            if (command.Is(Command.Wisper))
            {
                HandleWisperCommand(login, command);
                return;
            }

            if (command.Is(Command.Help))
            {
                HandleHelpCommand(login, command);
                return;
            }
        }

        private void HandleHelpCommand(string login, ServerCommand command)
        {
            if (command.HasFurtherParts)
                ShowDetailedHelp(login, command);
            else
                ShowHelpSummary(login);
        }

        private void ShowHelpSummary(string login)
        {
            SendHelpListPageToLogin(login, 0);

            //List<CommandHelp> helpList = GetPluginsCommandHelpList(login);

            //StringBuilder message = new StringBuilder();
            //message.Append("{[#ServerStyle]}>{[#MessageStyle]} Type '/help xxx' to get a detailed help for a command. Commands are: ");

            //for (int i = 0; i < helpList.Count; i++)
            //{
            //    message.AppendFormat((i != 0 ? ", " : string.Empty) + "{0}", helpList[i].CommandName);
            //}

            //SendFormattedMessageToLogin(login, message.ToString());
        }

        private void ShowDetailedHelp(string login, ServerCommand command)
        {
            string commandName = command.PartsWithoutMainCommand[0];
            List<CommandHelp> helpList = GetPluginsCommandHelpList(login);

            CommandHelp commandHelp = helpList.Find(c => (string.Compare(commandName, c.CommandName, StringComparison.InvariantCultureIgnoreCase) == 0) || Array.Exists(c.AlternativeCommandNames, acn => string.Compare(commandName, acn, StringComparison.InvariantCultureIgnoreCase) == 0));

            if (commandHelp != null)
            {
                string message = commandHelp.Description;

                if (!commandHelp.Usage.IsNullOrTimmedEmpty())
                    message = string.Concat(message, " Usage: ", commandHelp.Usage, ".");

                if (!commandHelp.UsageExample.IsNullOrTimmedEmpty())
                    message = string.Concat(message, " Usage-Example: ", commandHelp.UsageExample, ".");

                if (commandHelp.AlternativeCommandNames != null && commandHelp.AlternativeCommandNames.Length > 0)
                    message = string.Concat(message, " Alternative command(s): ", string.Join(", ", commandHelp.AlternativeCommandNames));

                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} {[Message]}", "Message", message);
            }
            else
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} Command '{[CommandName]}' does not exist.", "CommandName", commandName);
        }

        private void HandleWisperCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count < 2)
                return;

            string loginToWisperTo = command.PartsWithoutMainCommand[0].Trim();
            string wisperText = string.Join(" ", command.PartsWithoutMainCommand.ToArray(), 1, command.PartsWithoutMainCommand.Count - 1);

            WisperToLogin(login, loginToWisperTo, wisperText);
        }

        public void WisperToLogin(string login, string loginToWisperTo, string message)
        {
            if (GetPlayerSettings(loginToWisperTo) == null)
            {
                SendNoPlayerWithLoginMessageToLogin(login, loginToWisperTo);
                return;
            }

            string sourceNickname = GetNickname(login, true);
            string targetNickname = GetNickname(loginToWisperTo, true);

            SendFormattedMessageToLogin(login, Settings.WisperSourceMessage, "Nickname", targetNickname, "Message", message);
            SendFormattedMessageToLogin(loginToWisperTo, Settings.WisperTargetMessage, "Nickname", sourceNickname, "Message", message);
        }

        private void HandleWarnCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginToWarn = command.PartsWithoutMainCommand[0].Trim();

            WarnLogin(login, loginToWarn);
        }

        public void WarnLogin(string operatorLogin, string loginToWarn)
        {
            if (!LoginHasRight(operatorLogin, true, Command.Warn))
                return;

            if (LoginHasRight(loginToWarn, false, Right.WARN_PROTECTION))
                return;

            string nickname = GetNickname(operatorLogin);

            if (nickname == null)
                return;

            string nicknameToWarn = GetNickname(loginToWarn);

            if (nicknameToWarn != null)
            {
                SendFormattedMessage(Settings.PublicWarnMessage, "WarningNickname", StripTMColorsAndFormatting(nickname), "WarnedNickname", StripTMColorsAndFormatting(nicknameToWarn));
                Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(loginToWarn, FormatMessage(Settings.WarnManiaLinkPageContent), (int) Settings.WarnTimeout * 1000, false);
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(operatorLogin, loginToWarn);
            }
        }

        private void HandleReadCredentialsCommand(string login)
        {
            if (!LoginHasRight(login, true, Command.ReadCredentials))
                return;

            try
            {
                Context.Credentials.ReReadFromFile();
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} Credentials were successfully read from file!");
            }
            catch (Exception ex)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#ErrorStyle]} Error reading credentials from file.");
                Logger.Error("Error reading credentials from file.", ex);
                throw;
            }
        }

        private void HandleForceSpectatorCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginToForce = command.PartsWithoutMainCommand[0].Trim();
            ForceSpectatorLogin(login, loginToForce);
        }

        public void ForceSpectatorLogin(string operatorLogin, string loginToForce)
        {
            if (!LoginHasRight(operatorLogin, true, Command.ForceSpectator))
                return;

            if (LoginHasRight(loginToForce, false, Right.FORCE_SPECTATOR_PROTECTION))
                return;

            string nickname = GetNickname(operatorLogin);

            if (nickname == null)
                return;

            string nicknameToForce = GetNickname(loginToForce);

            if (nicknameToForce != null)
            {
                GenericResponse<bool> forceResponse1 = Context.RPCClient.Methods.ForceSpectator(loginToForce, 1);
                GenericResponse<bool> forceResponse2 = Context.RPCClient.Methods.ForceSpectator(loginToForce, 0);

                if (!forceResponse1.Erroneous && forceResponse1.Value && !forceResponse2.Erroneous && forceResponse2.Value)
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#MessageStyle]} Successfully forced player {[#HighlightStyle]}{[Nickname]}{[#MessageStyle]} into spectator mode.", "Nickname", StripTMColorsAndFormatting(nicknameToForce));
                else
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]}Could not force {[#HighlightStyle]}{[Nickname]}{[#ErrorStyle]} into spectator mode.", "Nickname", StripTMColorsAndFormatting(nicknameToForce));
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(operatorLogin, loginToForce);
            }
        }

        private void HandleKickCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginToKick = command.PartsWithoutMainCommand[0].Trim();

            KickLogin(login, loginToKick);
        }

        public void KickLogin(string operatorLogin, string loginToKick)
        {
            if (!LoginHasRight(operatorLogin, true, Command.Kick))
                return;

            if (LoginHasRight(loginToKick, false, Right.KICK_PROTECTION))
                return;

            string nickname = GetNickname(operatorLogin);

            if (nickname == null)
                return;


            string nicknameToKick = GetNickname(loginToKick);

            if (nicknameToKick != null)
            {
                GenericResponse<bool> kickResponse = Context.RPCClient.Methods.Kick(loginToKick, "You were kicked because of wrong behaviour!");

                if (!kickResponse.Erroneous && kickResponse.Value)
                    SendFormattedMessage(Settings.KickMessage, "KickingNickname", StripTMColorsAndFormatting(nickname), "KickedNickname", StripTMColorsAndFormatting(nicknameToKick));
                else
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]}Could not kick " + StripTMColorsAndFormatting(nicknameToKick));
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(operatorLogin, loginToKick);
            }
        }

        private void HandleBanCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginToBan = command.PartsWithoutMainCommand[0].Trim();
            BanLogin(login, loginToBan);
        }

        public void BanLogin(string operatorLogin, string loginToBan)
        {
            if (!LoginHasRight(operatorLogin, true, Command.Ban))
                return;

            if (LoginHasRight(loginToBan, false, Right.BAN_PROTECTION))
                return;

            string nickname = GetNickname(operatorLogin);

            if (nickname == null)
                return;


            string nickToBan = GetNickname(loginToBan);

            if (nickToBan != null)
            {
                GenericResponse<bool> banResponse = Context.RPCClient.Methods.Ban(loginToBan, "You were banned because of wrong behaviour!");

                if (!banResponse.Erroneous && banResponse.Value)
                    SendFormattedMessage(Settings.BanMessage, "BanningNickname", StripTMColorsAndFormatting(nickname), "BannedNickname", StripTMColorsAndFormatting(nickToBan));
                else
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]}Could not ban " + StripTMColorsAndFormatting(nickToBan));
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(operatorLogin, loginToBan);
            }
        }

        private void HandleUnBanCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginToRemove = command.PartsWithoutMainCommand[0].Trim();
            UnBanLogin(login, loginToRemove);
        }

        public void UnBanLogin(string operatorLogin, string loginToRemove)
        {
            if (!LoginHasRight(operatorLogin, true, Command.Unban))
                return;

            GenericResponse<bool> removePlayerResponse = Context.RPCClient.Methods.UnBan(loginToRemove);

            string nickname = GetNickname(loginToRemove) ?? loginToRemove;

            if (removePlayerResponse.Erroneous || !removePlayerResponse.Value)
                SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]} Could not unban " + StripTMColorsAndFormatting(nickname) + ".");
            else
                SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#MessageStyle]} Successfully removed player {[#HighlightStyle]}{[Nickname]}{[#MessageStyle]} from ban list.", "Nickname", StripTMColorsAndFormatting(nickname));
        }

        private void HandleIgnoreCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginToIgnore = command.PartsWithoutMainCommand[0].Trim();
            IgnoreLogin(login, loginToIgnore);
        }

        public void IgnoreLogin(string operatorLogin, string loginToIgnore)
        {
            if (!LoginHasRight(operatorLogin, true, Command.Ignore))
                return;

            if (LoginHasRight(loginToIgnore, false, Right.IGNORE_PROTECTION))
                return;

            string nickname = GetNickname(operatorLogin);

            if (nickname == null)
                return;

            string nickToIgnore = GetNickname(loginToIgnore);

            if (nickToIgnore != null)
            {
                GenericResponse<bool> ignoreResponse = Context.RPCClient.Methods.Ignore(loginToIgnore);

                if (!ignoreResponse.Erroneous && ignoreResponse.Value)
                    SendFormattedMessage(Settings.IgnoreMessage, "IgnoringNickname", StripTMColorsAndFormatting(nickname), "IgnoredNickname", StripTMColorsAndFormatting(nickToIgnore));
                else
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]}Could not ignore " + StripTMColorsAndFormatting(nickToIgnore));
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(operatorLogin, loginToIgnore);
            }
        }

        private void HandleUnIgnoreCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginToRemove = command.PartsWithoutMainCommand[0].Trim();
            UnIgnoreLogin(login, loginToRemove);
        }

        public void UnIgnoreLogin(string operatorLogin, string loginToRemove)
        {
            if (!LoginHasRight(operatorLogin, true, Command.Ignore))
                return;

            GenericResponse<bool> removePlayerResponse = Context.RPCClient.Methods.UnIgnore(loginToRemove);

            string nickname = GetNickname(loginToRemove) ?? loginToRemove;

            if (removePlayerResponse.Erroneous || !removePlayerResponse.Value)
                SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]} Could not unignore " + StripTMColorsAndFormatting(nickname) + ".");
            else
                SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#MessageStyle]} Successfully removed player {[#HighlightStyle]}{[Nickname]}{[#MessageStyle]} from ignore list.", "Nickname", StripTMColorsAndFormatting(nickname));
        }

        private void HandleAddGuestCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginOfGuest = command.PartsWithoutMainCommand[0].Trim();
            AddGuestLogin(login, loginOfGuest);
        }

        public void AddGuestLogin(string operatorLogin, string loginOfGuest)
        {
            if (!LoginHasRight(operatorLogin, true, Command.AddGuest))
                return;

            string nickname = GetNickname(operatorLogin);

            if (nickname == null)
                return;


            string nickofGuest = GetNickname(loginOfGuest);

            if (nickofGuest != null)
            {
                GenericResponse<bool> ignoreResponse = Context.RPCClient.Methods.AddGuest(loginOfGuest);

                if (!ignoreResponse.Erroneous && ignoreResponse.Value)
                {
                    Context.RPCClient.Methods.SaveGuestList("guestlist.txt");
                    SendFormattedMessage(Settings.AddGuestMessage, "AdminNickname", StripTMColorsAndFormatting(nickname), "GuestNickname", StripTMColorsAndFormatting(nickofGuest));
                }
                else
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]}Could not add " + StripTMColorsAndFormatting(nickofGuest) + "to guest list");
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(operatorLogin, loginOfGuest);
            }
        }

        private void HandleRemoveGuestCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginToRemove = command.PartsWithoutMainCommand[0].Trim();
            RemoveGuestLogin(login, loginToRemove);
        }

        public void RemoveGuestLogin(string operatorLogin, string loginToRemove)
        {
            if (!LoginHasRight(operatorLogin, true, Command.AddGuest))
                return;

            GenericResponse<bool> removeGuestResponse = Context.RPCClient.Methods.RemoveGuest(loginToRemove);
            string nickname = GetNickname(loginToRemove) ?? loginToRemove;

            if (removeGuestResponse.Erroneous || !removeGuestResponse.Value)
            {
                SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]} Could not remove " + StripTMColorsAndFormatting(nickname) + " from guestlist.");
            }

            if (!removeGuestResponse.Erroneous && removeGuestResponse.Value)
            {
                GenericResponse<bool> saveGuestListResponse = Context.RPCClient.Methods.SaveGuestList("guestlist.txt");

                if (saveGuestListResponse.Erroneous || !saveGuestListResponse.Value)
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]} Could not update guest list.txt");
                else
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#MessageStyle]} Successfully removed player {[#HighlightStyle]}{[Nickname]}{[#MessageStyle]} from guest list.", "Nickname", StripTMColorsAndFormatting(nickname));
            }
        }

        private void HandleBlackListCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginToBlacklist = command.PartsWithoutMainCommand[0].Trim();
            AddBlackListLogin(login, loginToBlacklist);
        }

        public void AddBlackListLogin(string operatorLogin, string loginToBlacklist)
        {
            if (!LoginHasRight(operatorLogin, true, Command.Blacklist))
                return;

            if (LoginHasRight(loginToBlacklist, false, Right.BLACKLIST_PROTECTION))
                return;

            string nickname = GetNickname(operatorLogin);

            if (nickname == null)
                return;


            string nicknameToBlacklist = GetNickname(loginToBlacklist);

            if (nicknameToBlacklist != null)
            {
                GenericResponse<bool> blacklistResponse = Context.RPCClient.Methods.BlackList(loginToBlacklist);

                if (!blacklistResponse.Erroneous && blacklistResponse.Value)
                {
                    Context.RPCClient.Methods.SaveBlackList("blacklist.txt");
                    SendFormattedMessage(Settings.BlackListMessage, "BlackListingNickname", StripTMColorsAndFormatting(nickname), "BlackListedNickname", StripTMColorsAndFormatting(nicknameToBlacklist));
                }
                else
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]}Could not blacklist " + StripTMColorsAndFormatting(nicknameToBlacklist));
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(operatorLogin, loginToBlacklist);
            }
        }

        private void HandleUnBlackListCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            string loginToRemove = command.PartsWithoutMainCommand[0].Trim();
            RemoveBlackListLogin(login, loginToRemove);
        }

        public void RemoveBlackListLogin(string operatorLogin, string loginToRemove)
        {
            if (!LoginHasRight(operatorLogin, true, Command.Blacklist))
                return;

            GenericResponse<bool> removeBlackResponse = Context.RPCClient.Methods.UnBlackList(loginToRemove);

            string nickname = GetNickname(loginToRemove) ?? loginToRemove;

            if (removeBlackResponse.Erroneous || !removeBlackResponse.Value)
            {
                SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]} Could not remove " + StripTMColorsAndFormatting(nickname) + " from black list.");
            }

            if (!removeBlackResponse.Erroneous && removeBlackResponse.Value)
            {
                GenericResponse<bool> saveBlackListResponse = Context.RPCClient.Methods.SaveBlackList("Blacklist.txt");

                if (saveBlackListResponse.Erroneous || !saveBlackListResponse.Value)
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#ErrorStyle]} Could not update Blacklist.txt");
                else
                    SendFormattedMessageToLogin(operatorLogin, "{[#ServerStyle]}> {[#MessageStyle]} Successfully removed player {[#HighlightStyle]}{[Nickname]}{[#MessageStyle]} from black list.", "Nickname", StripTMColorsAndFormatting(nickname));
            }
        }

        private void HandleWriteTrackListCommand(string login, ServerCommand command)
        {
            if (!LoginHasRight(login, true, Command.WriteTrackList))
                return;

            string filename = Settings.TrackListFile;

            if (command.PartsWithoutMainCommand.Count > 0)
                filename = command.PartsWithoutMainCommand[0] + ".txt";

            GenericResponse<int> response = Context.RPCClient.Methods.SaveMatchSettings("MatchSettings/"+filename);

            if (!response.Erroneous)
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#HighlightStyle]}{[Count]}{[#MessageStyle]} Tracks written!", "Count", response.Value.ToString());
            else
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]} error writing tracklist.");
        }

        private void HandleReadTrackListCommand(string login, ServerCommand command)
        {
            if (!LoginHasRight(login, true, Command.ReadTrackList))
                return;

            string filename = Settings.TrackListFile;

            if (command.PartsWithoutMainCommand.Count > 0)
                filename = command.PartsWithoutMainCommand[0] + ".txt";

            GenericResponse<int> response = Context.RPCClient.Methods.LoadMatchSettings("MatchSettings/" + filename);

            if (!response.Erroneous)
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#HighlightStyle]}{[Count]}{[#MessageStyle]} Tracks read!", "Count", response.Value.ToString());
            else
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]} error reading tracklist.");
        }

        private void HandleRemoveTrackCommand(string login)
        {
            if (!LoginHasRight(login, true, Command.RemoveCurrentTrack))
                return;

            ChallengeListSingleInfo challengeInfo = GetCurrentChallengeInfoCached();

            if (challengeInfo == null)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]} Could not retrieve current challenge info.");
                return;
            }

            GenericResponse<bool> response = Context.RPCClient.Methods.RemoveChallenge(challengeInfo.FileName);

            if (!response.Erroneous && response.Value)
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#MessageStyle]} Removed track {[#HighlightStyle]}{[Track]}{[#MessageStyle]}. Use {[#HighlightStyle]}WriteTrackList{[#MessageStyle]} command to save the changes!", "Track", StripTMColorsAndFormatting(challengeInfo.Name));
            else
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]} Error removing current track.");
        }

        public void KickAllSpectators(string login)
        {
            if (!LoginHasRight(login, true, Command.KickSpectators))
                return;

            List<PlayerSettings> playerSettings = Context.PlayerSettings.GetAsList(playerSetting => playerSetting.SpectatorStatus.IsSpectator);
            int playersKickedCount = 0;

            foreach (PlayerSettings playerSetting in playerSettings)
            {
                if (LoginHasRight(playerSetting.Login, false, Right.KICK_PROTECTION))
                    continue;

                Context.RPCClient.Methods.Kick(playerSetting.Login, "Kicked for spectating without asking.");
                SendFormattedMessage("{[#ServerStyle]}>> {[#HighlightStyle]}" + StripTMColorsAndFormatting(playerSetting.NickName) + "{[#MessageStyle]} got kicked for spectating without asking.");
                playersKickedCount++;
            }

            if (playersKickedCount == 0)
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} No one is spectating!");
            else
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} Kicked {[#HighlightStyle]}" + playersKickedCount + "{[#MessageStyle]} player for spectating without asking.");
        }

        public void KickSpectatorsOf(string login, int playerID)
        {
            if (!LoginHasRight(login, true, Command.KickMySpectators))
                return;

            List<PlayerSettings> playerSettings = Context.PlayerSettings.GetAsList(playerSetting => playerSetting.SpectatorStatus.IsSpectator && playerSetting.SpectatorStatus.CurrentPlayerTargetID == playerID);
            int playersKickedCount = 0;

            foreach (PlayerSettings playerSetting in playerSettings)
            {
                if (LoginHasRight(playerSetting.Login, false, Right.KICK_PROTECTION))
                    continue;

                Context.RPCClient.Methods.Kick(playerSetting.Login, "Kicked for spectating without asking.");
                SendFormattedMessage("{[#ServerStyle]}>> {[#HighlightStyle]}" + StripTMColorsAndFormatting(playerSetting.NickName) + "{[#MessageStyle]} got kicked for spectating without asking.");
                playersKickedCount++;
            }

            if (playersKickedCount == 0)
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} No one is spectating you!");
            else
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} Kicked {[#HighlightStyle]}" + playersKickedCount + "{[#MessageStyle]} player for spectating without asking.");
        }

        //private void HandleRestartServerCommand(ServerCommand command)
        //{
        //    int timeoutInSeconds = 0;

        //    if (command.PartsWithoutMainCommand.Count > 0)
        //        int.TryParse(command.PartsWithoutMainCommand[0], NumberStyles.None, Context.Culture, out timeoutInSeconds);

        //    Context.RPCClient.Methods.StopServer();
        //    Context.RPCClient.Methods.StartServerInternet(Context.ServerInfo.ServerLogin, Context.ServerInfo.ServerLoginPassword);
        //    //Context.RPCClient.Methods.StartServerLan();
        //}


        private List<CommandHelp> GetPluginsCommandHelpList(string login)
        {
            Dictionary<string, CommandHelp> commandHelpDictionary = new Dictionary<string, CommandHelp>();

            foreach (ITMSPSPlugin plugin in Context.Plugins)
            {
                IEnumerable<CommandHelp> pluginCommandHelpList = plugin.CommandHelpList;

                if (pluginCommandHelpList == null)
                    continue;

                foreach (CommandHelp commandHelp in pluginCommandHelpList)
                {
                    if (login != null && commandHelp.NecessaryRights != null && commandHelp.NecessaryRights.Length > 0 && !LoginHasAnyRight(login, false, commandHelp.NecessaryRights))
                        continue;

                    commandHelpDictionary[commandHelp.CommandName.ToLower(CultureInfo.InvariantCulture)] = commandHelp;
                }
            }

            List<CommandHelp> result = new List<CommandHelp>();

            foreach (KeyValuePair<string, CommandHelp> commandHelpInfo in commandHelpDictionary)
            {
                result.Add(commandHelpInfo.Value);
            }

            result.Sort((x, y) => string.Compare(x.CommandName, y.CommandName, StringComparison.InvariantCultureIgnoreCase));

            return result;
        }

        public override IEnumerable<CommandHelp> CommandHelpList
        {
            get
            {
                return new[]
                {
                    new CommandHelp(Command.Kick, "Kicks a player with the specified login.", "/t kick <login>", "/t kick scordo"),
                    new CommandHelp(Command.Warn, "Warns a player with the specified login.", "/t warn <login>", "/t warn scordo"),
                    new CommandHelp(Command.Ban, "Bans a player with the specified login", "/t ban <login>", "/t ban scordo"),
                    new CommandHelp(Command.Unban, "Removes the ban of a player with the specified login.", "/t unban <login>", "/t unban scordo"),
                    new CommandHelp(Command.Blacklist, "Blacklists a player with the specified login.", "/t blacklist <login>", "/t blacklist scordo"),
                    new CommandHelp(Command.Unblacklist, "Removes the blacklist entry of a player with the specified login.", "/t unblacklist <login>", "/t unblacklist scordo"),
                    new CommandHelp(Command.Ignore, "Ignores a player with the specified login.", "/t ignore <login>", "/t ignore scordo"),
                    new CommandHelp(Command.Unignore, "Removes the ignore of a player with the specified login", "/t unignore <login>", "/t unignore scordo"),
                    new CommandHelp(Command.AddGuest, "Add the player with the specified login to the guest list.", "/t addguest <login>", "/t addguest scordo"),
                    new CommandHelp(Command.RemoveGuest, "Removes the player with the specified login from the guest list.", "/t removeguest <login>", "/t removeguest scordo"),
                    new CommandHelp(Command.ForceSpectator, "Forces the player with the specified login into spectator mode.", "/t forcespectator <login>", "/t forcespectator scordo"),
                    new CommandHelp(Command.WriteTrackList, "Writes the current tracklist to the standard tracklist file on disk.", "/t writetracklist", "/t writetracklist"),
                    new CommandHelp(Command.ReadTrackList, "Reads the the standard tracklist file from disk and replaces the current in memory tracklist.", "/t readtracklist", "/t readtracklist"),
                    new CommandHelp(Command.RemoveCurrentTrack, "Removes the currently playing track from the current in memory tracklist. You need to call 'writetracklist' to commit the changes.", "/t removecurrenttrack", "/t removecurrenttrack"),
                    new CommandHelp(Command.Wisper, "Writes a private message to the player with the specified login.", "/pm <login> <message>", "/pm scordo hello"),
                    new CommandHelp(Command.ReadCredentials, "Refreshes user credentials stored in memory by reading from credentials.xml.", "/t readcredentials", "/t readcredentials"),
                };
            }
        }

        protected override void OnManiaLinkPageAnswer(string login, int playerID, TMAction action)
        {
            switch ((Area)action.AreaID)
            {
                case Area.Main:
                    HandleMainAreaActions(login, playerID, action);
                    break;
                case Area.HelpList:
                    HandleHelpListAreaActions(login, action);
                    break;
            }
        }

        private void HandleMainAreaActions(string login, int playerID, TMAction action)
        {
            
        }

        private void HandleHelpListAreaActions(string login, TMAction areaAction)
        {
            if (areaAction.IsAreaAction)
            {
                PagedDialogActions.DefaultDialogAction action = (PagedDialogActions.DefaultDialogAction)areaAction.AreaActionID;

                switch (action)
                {
                    case PagedDialogActions.DefaultDialogAction.CloseDialog:
                        GetPluginSettings(login).AreaSettings.Reset((byte)Area.HelpList);
                        SendEmptyManiaLinkPageToLogin(login, HELP_LIST_PANEL_ID);
                        break;
                    case PagedDialogActions.DefaultDialogAction.FirstPage:
                        SendHelpListPageToLogin(login, 0);
                        break;
                    case PagedDialogActions.DefaultDialogAction.PrevPage:
                        ushort prevPageIndex = Convert.ToUInt16(Math.Max(0, GetAreaSettings(login, (byte)Area.HelpList).CurrentDialogPageIndex - 1));
                        SendHelpListPageToLogin(login, prevPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.NextPage:

                        ushort nextPageIndex = Convert.ToUInt16(GetAreaSettings(login, (byte)Area.HelpList).CurrentDialogPageIndex + 1);
                        SendHelpListPageToLogin(login, nextPageIndex);
                        break;
                    case PagedDialogActions.DefaultDialogAction.LastPage:
                        SendHelpListPageToLogin(login, null);
                        break;
                }
            }
            else
            {
                //GuestListRowAction action = (GuestListRowAction)areaAction.RowActionID;

                //switch (action)
                //{
                //    case GuestListRowAction.RemovePlayer:
                //        RemoveGuestListPlayer(login, areaAction.RowIndex);
                //        break;
                //}
            }
        }

        private void SendHelpListPageToLogin(string login, uint? pageIndex)
        {
            List<CommandHelp> commandHelpList = GetPluginsCommandHelpList(login);

            uint maxPageIndex = Convert.ToUInt32(Math.Max(0, Math.Ceiling((double)commandHelpList.Count / HelpSettings.MaxEntriesPerPage) - 1));

            if (!pageIndex.HasValue)
                pageIndex = maxPageIndex;

            pageIndex = Convert.ToUInt16(Math.Min(Math.Max(0, (int)pageIndex), maxPageIndex));
            GetAreaSettings(login, (byte)Area.HelpList).CurrentDialogPageIndex = (ushort)pageIndex;

            int entriesToSkip = Convert.ToInt32(pageIndex * HelpSettings.MaxEntriesPerPage);
            List<CommandHelp> helpCommandListEntriesToShow = commandHelpList.Skip(entriesToSkip).Take(Convert.ToInt32(HelpSettings.MaxEntriesPerPage)).ToList();

            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetCommandListManiaLinkPage(pageIndex.Value + 1, maxPageIndex + 1, helpCommandListEntriesToShow), 0, false);
        }

        private string GetCommandListManiaLinkPage(uint currentPage, uint maxPage, IEnumerable<CommandHelp> commands)
        {
            string mainTemplateString = HelpSettings.SinglePageTemplate;

            if (maxPage > 1)
            {
                if (currentPage == 1)
                    mainTemplateString = HelpSettings.FirstPageTemplate;
                else
                    mainTemplateString = currentPage == maxPage ? HelpSettings.LastPageTemplate : HelpSettings.MiddlePageTemplate;
            }

            mainTemplateString = ReplaceMessagePlaceHolders(mainTemplateString, HelpListActions.GetReplaceParameters());

            XElement mainTemplate = XElement.Parse(FormatMessage(mainTemplateString, "ManiaLinkID", HELP_LIST_PANEL_ID, "CurrentPage", currentPage.ToString(CultureInfo.InvariantCulture), "MaxPage", maxPage.ToString(CultureInfo.InvariantCulture)));
            XElement entryPlaceHolder = mainTemplate.Descendants("HelpPlaceHolder").First();
            double currentY = HelpSettings.FirstEntryTopMargin;

            XElement lastInsertedNode = entryPlaceHolder;

            foreach (CommandHelp commandHelpEntry in commands)
            {
                XElement currentElement = GetHelpListElement(commandHelpEntry, currentY);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= HelpSettings.EntryHeight;
            }

            entryPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private XElement GetHelpListElement(CommandHelp commandHelpEntry, double currentY)
        {
            List<string> commandNames = new List<string>(commandHelpEntry.AlternativeCommandNames ?? new string[] { });

            if (commandNames.Count == 0)
                commandNames.Add("-");

            return XElement.Parse
            (
                FormatMessage
                (
                    HelpSettings.EntryTemplate,
                    "Y", currentY.ToString(CultureInfo.InvariantCulture),
                    "CommandName", SecurityElement.Escape(commandHelpEntry.CommandName),
                    "Description", SecurityElement.Escape(commandHelpEntry.Description),
                    "Usage", SecurityElement.Escape(commandHelpEntry.Usage),
                    "UsageExample", SecurityElement.Escape(commandHelpEntry.UsageExample),
                    "Aliases", SecurityElement.Escape(string.Join(", ", commandNames.ToArray()))
                )
            );
        }

        #region Embedded Types

        private enum Area
        {
            Main = 1,
            HelpList = 2,
        }

        //private enum MainAreaAction
        //{
        //    ShowHelp
        //}

        #endregion
    }
}