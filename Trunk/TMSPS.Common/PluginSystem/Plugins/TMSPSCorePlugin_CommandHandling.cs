using System.IO;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.PluginSystem.Plugins
{
    internal partial class TMSPSCorePlugin
    {
        //const string COMMAND_RESTART_SERVER = "restartserver";
        private const string COMMAND_KICK = "kick";
        private const string COMMAND_BAN = "ban";
        private const string COMMAND_BLACKLIST = "blacklist";
        private const string COMMAND_IGNORE = "ignore";
        private const string COMMAND_ADDGUEST = "addguest";
        private const string COMMAND_WRITE_TRACK_LIST = "writetracklist";
        private const string COMMAND_READ_TRACK_LIST = "readtracklist";
        private const string COMMAND_REMOVE_TRACK = "removetrack";

        private void HandleCommand(string login, ServerCommand command)
        {
            switch (command.MainCommand.ToLower(Context.Culture))
            {
                case COMMAND_KICK:
                    HandleKickCommand(login, command);
                    break;
                case COMMAND_BAN:
                    HandleBanCommand(login, command);
                    break;
                case COMMAND_BLACKLIST:
                    HandleBlackListCommand(login, command);
                    break;
                case COMMAND_IGNORE:
                    HandleIgnoreCommand(login, command);
                    break;
                case COMMAND_ADDGUEST:
                    HandleAddGuestCommand(login, command);
                    break;
                case COMMAND_WRITE_TRACK_LIST:
                    HandleWriteTrackListCommand(login, command);
                    break;
                case COMMAND_READ_TRACK_LIST:
                    HandleReadTrackListCommand(login, command);
                    break;
                case COMMAND_REMOVE_TRACK:
                    HandleRemoveTrackCommand(login, command);
                    break;
                //case COMMAND_RESTART_SERVER:
                //    HandleRestartServerCommand(command);
                //    break;
            }
        }

        private void HandleKickCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            if (!Context.Credentials.UserHasAnyRight(login, COMMAND_KICK))
            {
                SendNoPermissionMessagetoLogin(login);
                return;
            }

            string nickname = GetNickname(login);

            if (nickname == null)
                return;

            string loginToKick = command.PartsWithoutMainCommand[0].Trim();
            string nicknameToKick = GetNickname(loginToKick);

            if (nicknameToKick != null)
            {
                GenericResponse<bool> kickResponse = Context.RPCClient.Methods.Kick(loginToKick, "You were kicked because of wrong behaviour!");

                if (!kickResponse.Erroneous && kickResponse.Value)
                    SendFormattedMessage(Settings.KickMessage, "KickingNickname", StripTMColorsAndFormatting(nickname), "KickedNickname", StripTMColorsAndFormatting(nicknameToKick));
                else
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not kick " + StripTMColorsAndFormatting(nicknameToKick));
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(login, loginToKick);
            }
        }

        private void HandleBanCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            if (!Context.Credentials.UserHasAnyRight(login, COMMAND_BAN))
            {
                SendNoPermissionMessagetoLogin(login);
                return;
            }

            string nickname = GetNickname(login);

            if (nickname == null)
                return;

            string loginToBan = command.PartsWithoutMainCommand[0].Trim();
            string nickToBan = GetNickname(loginToBan);

            if (nickToBan != null)
            {
                GenericResponse<bool> banResponse = Context.RPCClient.Methods.Ban(loginToBan, "You were banned because of wrong behaviour!");

                if (!banResponse.Erroneous && banResponse.Value)
                    SendFormattedMessage(Settings.BanMessage, "BanningNickname", StripTMColorsAndFormatting(nickname), "BannedNickname", StripTMColorsAndFormatting(nickToBan));
                else
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not ban " + StripTMColorsAndFormatting(nickToBan));
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(login, loginToBan);
            }
        }

        private void HandleIgnoreCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            if (!Context.Credentials.UserHasAnyRight(login, COMMAND_IGNORE))
            {
                SendNoPermissionMessagetoLogin(login);
                return;
            }

            string nickname = GetNickname(login);

            if (nickname == null)
                return;

            string loginToIgnore = command.PartsWithoutMainCommand[0].Trim();
            string nickToIgnore = GetNickname(loginToIgnore);

            if (nickToIgnore != null)
            {
                GenericResponse<bool> ignoreResponse = Context.RPCClient.Methods.Ignore(loginToIgnore);

                if (!ignoreResponse.Erroneous && ignoreResponse.Value)
                    SendFormattedMessage(Settings.IgnoreMessage, "IgnoringNickname", StripTMColorsAndFormatting(nickname), "IgnoredNickname", StripTMColorsAndFormatting(nickToIgnore));
                else
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not ignore " + StripTMColorsAndFormatting(nickToIgnore));
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(login, loginToIgnore);
            }
        }

        private void HandleAddGuestCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            if (!Context.Credentials.UserHasAnyRight(login, COMMAND_ADDGUEST))
            {
                SendNoPermissionMessagetoLogin(login);
                return;
            }

            string nickname = GetNickname(login);

            if (nickname == null)
                return;

            string loginOfGuest = command.PartsWithoutMainCommand[0].Trim();
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
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not add " + StripTMColorsAndFormatting(nickofGuest) + "to guest list");
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(login, loginOfGuest);
            }
        }

        private void HandleBlackListCommand(string login, ServerCommand command)
        {
            if (command.PartsWithoutMainCommand.Count == 0)
                return;

            if (!Context.Credentials.UserHasAnyRight(login, COMMAND_BLACKLIST))
            {
                SendNoPermissionMessagetoLogin(login);
                return;
            }

            string nickname = GetNickname(login);

            if (nickname == null)
                return;

            string loginToBlacklist = command.PartsWithoutMainCommand[0].Trim();
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
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not blacklist " + StripTMColorsAndFormatting(nicknameToBlacklist));
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(login, loginToBlacklist);
            }
        }

        private void HandleWriteTrackListCommand(string login, ServerCommand command)
        {
            if (!Context.Credentials.UserHasAnyRight(login, COMMAND_WRITE_TRACK_LIST))
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
            if (!Context.Credentials.UserHasAnyRight(login, COMMAND_READ_TRACK_LIST))
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

        private void HandleRemoveTrackCommand(string login, ServerCommand command)
        {
            if (!Context.Credentials.UserHasAnyRight(login, COMMAND_REMOVE_TRACK))
            {
                SendNoPermissionMessagetoLogin(login);
                return;
            }

            ChallengeListSingleInfo challengeInfo = GetCurrentChallengeInfo();

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
