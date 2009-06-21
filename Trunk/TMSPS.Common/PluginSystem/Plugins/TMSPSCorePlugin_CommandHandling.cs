using System;
using System.IO;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.PluginSystem.Plugins
{
    public partial class TMSPSCorePlugin
    {
        

        private void HandleCommand(string login, ServerCommand command)
        {
            switch (command.MainCommand.ToLower(Context.Culture))
            {
                case CommandOrRight.KICK:
                    HandleKickCommand(login, command);
                    break;
                case CommandOrRight.BAN:
                    HandleBanCommand(login, command);
                    break;
                case CommandOrRight.UNBAN:
                    HandleUnBanCommand(login, command);
                    break;
                case CommandOrRight.BLACKLIST:
                    HandleBlackListCommand(login, command);
                    break;
                case CommandOrRight.UNBLACKLIST:
                    HandleUnBlackListCommand(login, command);
                    break;
                case CommandOrRight.IGNORE:
                    HandleIgnoreCommand(login, command);
                    break;
                case CommandOrRight.UNIGNORE:
                    HandleUnIgnoreCommand(login, command);
                    break;
                case CommandOrRight.ADD_GUEST:
                    HandleAddGuestCommand(login, command);
                    break;
                case CommandOrRight.REMOVE_GUEST:
                    HandleRemoveGuestCommand(login, command);
                    break;
                case CommandOrRight.FORCE_SPECTATOR:
                    HandleForceSpectatorCommand(login, command);
                    break;
                case CommandOrRight.WRITE_TRACK_LIST:
                    HandleWriteTrackListCommand(login, command);
                    break;
                case CommandOrRight.READ_TRACK_LIST:
                    HandleReadTrackListCommand(login, command);
                    break;
                case CommandOrRight.REMOVE_CURRENT_TRACK:
                    HandleRemoveTrackCommand(login);
                    break;
                case CommandOrRight.READ_CREDENTIALS:
                    HandleReadCredentialsCommand(login);
                    break;
                //case COMMAND_RESTART_SERVER:
                //    HandleRestartServerCommand(command);
                //    break;
            }
        }

        private void HandleReadCredentialsCommand(string login)
        {
            if (!Context.Credentials.UserHasAnyRight(login, CommandOrRight.READ_CREDENTIALS))
            {
                SendNoPermissionMessagetoLogin(login);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(operatorLogin, CommandOrRight.FORCE_SPECTATOR))
            {
                SendNoPermissionMessagetoLogin(operatorLogin);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(operatorLogin, CommandOrRight.KICK))
            {
                SendNoPermissionMessagetoLogin(operatorLogin);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(operatorLogin, CommandOrRight.BAN))
            {
                SendNoPermissionMessagetoLogin(operatorLogin);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(operatorLogin, CommandOrRight.BAN))
            {
                SendNoPermissionMessagetoLogin(operatorLogin);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(operatorLogin, CommandOrRight.IGNORE))
            {
                SendNoPermissionMessagetoLogin(operatorLogin);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(operatorLogin, CommandOrRight.IGNORE))
            {
                SendNoPermissionMessagetoLogin(operatorLogin);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(operatorLogin, CommandOrRight.ADD_GUEST))
            {
                SendNoPermissionMessagetoLogin(operatorLogin);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(operatorLogin, CommandOrRight.ADD_GUEST))
            {
                SendNoPermissionMessagetoLogin(operatorLogin);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(operatorLogin, CommandOrRight.BLACKLIST))
            {
                SendNoPermissionMessagetoLogin(operatorLogin);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(operatorLogin, CommandOrRight.BLACKLIST))
            {
                SendNoPermissionMessagetoLogin(operatorLogin);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(login, CommandOrRight.WRITE_TRACK_LIST))
            {
                SendNoPermissionMessagetoLogin(login);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(login, CommandOrRight.READ_TRACK_LIST))
            {
                SendNoPermissionMessagetoLogin(login);
                return;
            }

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
            if (!Context.Credentials.UserHasAnyRight(login, CommandOrRight.REMOVE_CURRENT_TRACK))
            {
                SendNoPermissionMessagetoLogin(login);
                return;
            }

            ChallengeListSingleInfo challengeInfo = GetCurrentChallengeInfoCached();

            if (challengeInfo == null)
            {
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]} Could not retrieve current challenge info.");
                return;
            }

            GenericResponse<bool> response = Context.RPCClient.Methods.RemoveChallenge(challengeInfo.FileName);

            if (!response.Erroneous && response.Value)
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#MessageStyle]} Removed track {[#HighlightStyle]}{[Track]}{[#MessageStyle]}. Use {[#HighlightStyle]}WriteTrackList{[#MessageStyle]} command to save the changes!", "Track", StripTMColorsAndFormatting(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(challengeInfo.FileName))));
            else
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]} Error removing current track.");
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
    }
}
