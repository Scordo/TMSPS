﻿namespace TMSPS.Core.Common
{
    public static class Command
    {
        //const string COMMAND_RESTART_SERVER = "restartserver";
        public const string KICK = "kick";
        public const string BAN = "ban";
        public const string UNBAN = "unban";
        public const string BLACKLIST = "blacklist";
        public const string UNBLACKLIST = "unblacklist";
        public const string IGNORE = "ignore";
        public const string UNIGNORE = "unignore";
        public const string ADD_GUEST = "addguest";
        public const string REMOVE_GUEST = "removeguest";
        public const string FORCE_SPECTATOR = "forcespectator";
        public const string WRITE_TRACK_LIST = "writetracklist";
        public const string READ_TRACK_LIST = "readtracklist";
        public const string REMOVE_CURRENT_TRACK = "removetrack";
        public const string READ_CREDENTIALS = "readcredentials";
        public const string GET_SPECTATORS1 = "getspectators";
        public const string GET_SPECTATORS2 = "specs";
        public const string KICK_SPECTATORS1 = "kickspectators";
        public const string KICK_SPECTATORS2 = "kickspecs";
        public const string READCLANTAG_SETTINGS = "readclantagsettings";
        public const string READ_CHATBOT_SETTINGS = "readchatbotsettings";
        public const string TMX_INFO = "tmxinfo";
        public const string TMX_ADD_TRACK = "addtrack";
        public const string TOPSUMS = "/topsums";
        public const string Summary = "/summary";
        public const string Info = "/info";
        public const string Wins = "/wins";
        public const string Played = "/played";
        public const string Visit = "/visit";
        public const string DELETE_CHEATER = "deletecheater";
        public const string GET_LOCAL_LOGINS = "getlocallogins";
        public const string RESTART_TRACK_IMMEDIATELY = "restarttrack";
        public const string NEXT_TRACK = "nexttrack";
        public const string SERVER_RANK1 = "/sr";
        public const string SERVER_RANK2 = "/rank";
        public const string NEXT_SERVER_RANK1 = "/nsr";
        public const string NEXT_SERVER_RANK2 = "/nextrank";
    }
}