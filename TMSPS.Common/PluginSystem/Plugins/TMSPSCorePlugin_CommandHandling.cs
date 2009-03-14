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

            PlayerInfo playerInfo = GetPlayerInfoCached(login);

            if (playerInfo == null)
                return;

            string loginToKick = command.PartsWithoutMainCommand[0].Trim();
            PlayerInfo playerToKickInfo = GetPlayerInfoCached(loginToKick);

            if (playerToKickInfo != null)
            {
                GenericResponse<bool> kickResponse = Context.RPCClient.Methods.Kick(loginToKick, "You were kicked because of wrong behaviour!");

                if (!kickResponse.Erroneous && kickResponse.Value)
                    SendFormattedMessage(Settings.KickMessage, "KickingNickname", StripTMColorsAndFormatting(playerInfo.NickName), "KickedNickname", StripTMColorsAndFormatting(playerToKickInfo.NickName));
                else
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not kick " + StripTMColorsAndFormatting(playerToKickInfo.NickName));
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

            PlayerInfo playerInfo = GetPlayerInfoCached(login);

            if (playerInfo == null)
                return;

            string loginToBan = command.PartsWithoutMainCommand[0].Trim();
            PlayerInfo playerToBanInfo = GetPlayerInfoCached(loginToBan);

            if (playerToBanInfo != null)
            {
                GenericResponse<bool> banResponse = Context.RPCClient.Methods.Ban(loginToBan, "You were banned because of wrong behaviour!");

                if (!banResponse.Erroneous && banResponse.Value)
                    SendFormattedMessage(Settings.BanMessage, "BanningNickname", StripTMColorsAndFormatting(playerInfo.NickName), "BannedNickname", StripTMColorsAndFormatting(playerToBanInfo.NickName));
                else
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not ban " + StripTMColorsAndFormatting(playerToBanInfo.NickName));
            }
            else
            {
                SendNoPlayerWithLoginMessageToLogin(login, loginToBan);
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

            PlayerInfo playerInfo = GetPlayerInfoCached(login);

            if (playerInfo == null)
                return;

            string loginToBlacklist = command.PartsWithoutMainCommand[0].Trim();
            PlayerInfo playerToBlacklistInfo = GetPlayerInfoCached(loginToBlacklist);

            if (playerToBlacklistInfo != null)
            {
                GenericResponse<bool> blacklistResponse = Context.RPCClient.Methods.BlackList(loginToBlacklist);

                if (!blacklistResponse.Erroneous && blacklistResponse.Value)
                {
                    SendFormattedMessage(Settings.BlackListMessage, "BlackListingNickname", StripTMColorsAndFormatting(playerInfo.NickName), "BlackListedNickname", StripTMColorsAndFormatting(playerToBlacklistInfo.NickName));
                }
                else
                    SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not blacklist " + StripTMColorsAndFormatting(playerToBlacklistInfo.NickName));
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
                SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#MessageStyle]} Removed track {[#HighlightStyle]}{[Track]}{[#MessageStyle]}. Use {[#HighlightStyle]}WriteTrackList{[#MessageStyle]} command to save the changes!", "Track", Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(challengeInfo.FileName)));
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
