﻿using System;
using System.Configuration;
using System.Xml.Linq;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins
{
    public class TMSPSCorePluginSettings : SettingsBase
    {
        #region Constants

        public const string JOIN_MESSAGE = "{[#ServerStyle]}>> {[#MessageStyle]}New Player: {[#HighlightStyle]}{[Nickname]}{[#MessageStyle]} Nation: {[#HighlightStyle]}{[Nation]}{[#MessageStyle]} Ladder: {[#HighlightStyle]}{[Ladder]}";
        public const bool ENABLE_JOIN_MESSAGE = true;
        public const string LEAVE_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[Nickname]}{[#MessageStyle]} has left the game.";
        public const bool ENABLE_LEAVE_MESSAGE = true;
        public const string KICK_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[KickingNickname]}{[#MessageStyle]} kicked {[#HighlightStyle]}{[KickedNickname]}.";
        public const string PUBLIC_WARN_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[WarningNickname]}{[#MessageStyle]} warned {[#HighlightStyle]}{[WarnedNickname]}.";
        public const string BAN_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[BanningNickname]}{[#MessageStyle]} banned {[#HighlightStyle]}{[BannedNickname]}.";
        public const string IGNORE_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[IgnoringNickname]}{[#MessageStyle]} added {[#HighlightStyle]}{[IgnoredNickname]}{[#MessageStyle]} to ignore list.";
        public const string ADDGUEST_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[AdminNickname]}{[#MessageStyle]} added {[#HighlightStyle]}{[GuestNickname]}{[#MessageStyle]} to guest list.";
        public const string BLACKLIST_MESSAGE = "{[#ServerStyle]}>> {[#HighlightStyle]}{[BlackListingNickname]}{[#MessageStyle]} blacklists {[#HighlightStyle]}{[BlackListedNickname]}.";
        public const string LOGIN_MISSING_MESSAGE = "{[#ServerStyle]}> {[#ErrorStyle]}There is no player with login {[#HighlightStyle]}{[Login]}.";
        public const string TRACKLIST_FILE = "tracklist.txt";
        public const string NICKNAME_RESOLVER_CLASS = "TMSPS.Core.Common.FlatFileNicknameResolver";
        public const string NICKNAME_RESOLVER_ASSEMBLY = "TMSPS.Core";
        public const uint SAVE_GHOST_REPLAY_OF_TOP = 3;
        public const bool ENABLE_DEDIMANIA_BLACKLIST_SYNC = true;
        public const uint DEDIMANIA_BLACKLIST_SYNC_INTERVAL = 60; // minutes
        public const string DEDIMANIA_BLACKLIST_URL = "http://www.gamers.org/tmf/dedimania_blacklist.txt";
        public const uint WARN_TIMEOUT = 15; // seconds

        #endregion

        #region Properties

        public bool EnableJoinMessage { get; private set; }
        public string JoinMessage { get; private set; }
        public string LeaveMessage { get; private set; }
        public bool EnableLeaveMessage { get; private set; }
        public string KickMessage { get; private set; }
        public string BanMessage { get; private set; }
        public string IgnoreMessage { get; private set; }
        public string AddGuestMessage { get; private set; }
        public string BlackListMessage { get; private set; }
        public string TrackListFile { get; private set; }
        public string NicknameResolverClass { get; private set; }
        public string NicknameResolverAssemblyLocation { get; private set;}
        public XElement NicknameResolverConfigElement { get; private set; }
        public uint SaveGhostReplayOfTop { get; private set; }
        public bool EnableDedimaniaBlackListSync { get; private set; }
        public TimeSpan DedimaniaBlackListSyncInterval { get; private set; }
        public string DedimaniaBlackListUrl { get; private set; }
        public string PublicWarnMessage { get; private set; }
        public string WarnManiaLinkPageContent { get; private set; }
        public string LoginMissingMessage { get; private set; }
        public uint WarnTimeout { get; private set; }

        #endregion

        #region Public Methods

        public static TMSPSCorePluginSettings ReadFromFile(string xmlConfigurationFile)
        {
            //string settingsDirectory = Path.GetDirectoryName(xmlConfigurationFile);

            TMSPSCorePluginSettings result = new TMSPSCorePluginSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.EnableJoinMessage = ReadConfigBool(configDocument.Root, "EnableJoinMessage", ENABLE_JOIN_MESSAGE, xmlConfigurationFile);
            result.JoinMessage = ReadConfigString(configDocument.Root, "JoinMessage", JOIN_MESSAGE, xmlConfigurationFile);
            result.EnableLeaveMessage = ReadConfigBool(configDocument.Root, "EnableLeaveMessage", ENABLE_LEAVE_MESSAGE, xmlConfigurationFile);
            result.LeaveMessage = ReadConfigString(configDocument.Root, "LeaveMessage", LEAVE_MESSAGE, xmlConfigurationFile);
            result.KickMessage = ReadConfigString(configDocument.Root, "KickMessage", KICK_MESSAGE, xmlConfigurationFile);
            result.BanMessage = ReadConfigString(configDocument.Root, "BanMessage", BAN_MESSAGE, xmlConfigurationFile);
            result.IgnoreMessage = ReadConfigString(configDocument.Root, "IgnoreMessage", IGNORE_MESSAGE, xmlConfigurationFile);
            result.AddGuestMessage = ReadConfigString(configDocument.Root, "AddGuestMessage", ADDGUEST_MESSAGE, xmlConfigurationFile);
            result.BlackListMessage = ReadConfigString(configDocument.Root, "BlackListMessage", BLACKLIST_MESSAGE, xmlConfigurationFile);
            result.TrackListFile = ReadConfigString(configDocument.Root, "TrackListFile", TRACKLIST_FILE, xmlConfigurationFile);
            result.SaveGhostReplayOfTop = ReadConfigUInt(configDocument.Root, "SaveGhostReplayOfTop", SAVE_GHOST_REPLAY_OF_TOP, xmlConfigurationFile);
            result.EnableDedimaniaBlackListSync = ReadConfigBool(configDocument.Root, "EnableDedimaniaBlackListSync", ENABLE_DEDIMANIA_BLACKLIST_SYNC, xmlConfigurationFile);
            result.DedimaniaBlackListSyncInterval = TimeSpan.FromMinutes(ReadConfigUInt(configDocument.Root, "DedimaniaBlackListSyncInterval", DEDIMANIA_BLACKLIST_SYNC_INTERVAL, xmlConfigurationFile));
            result.DedimaniaBlackListUrl = ReadConfigString(configDocument.Root, "DedimaniaBlackListUrl", DEDIMANIA_BLACKLIST_URL, xmlConfigurationFile);
            result.WarnManiaLinkPageContent = ReadConfigString(configDocument.Root, "WarnManiaLinkPageContent", xmlConfigurationFile).Replace("\t", string.Empty);
            result.PublicWarnMessage = ReadConfigString(configDocument.Root, "PublicWarnMessage", PUBLIC_WARN_MESSAGE, xmlConfigurationFile);
            result.WarnTimeout = ReadConfigUInt(configDocument.Root, "WarnTimeout", WARN_TIMEOUT, xmlConfigurationFile);
            result.LoginMissingMessage = ReadConfigString(configDocument.Root, "LoginMissingMessage", LOGIN_MISSING_MESSAGE, xmlConfigurationFile);
            

            result.NicknameResolverClass = NICKNAME_RESOLVER_CLASS;
            result.NicknameResolverAssemblyLocation = NICKNAME_RESOLVER_ASSEMBLY;
            result.NicknameResolverConfigElement = null;
            XElement nicknameResolverConfigElement = configDocument.Root.Element("NicknameResolver");

            if (nicknameResolverConfigElement != null)
            {
                result.NicknameResolverClass = ReadConfigString(nicknameResolverConfigElement, "Class", NICKNAME_RESOLVER_CLASS, xmlConfigurationFile); 
                result.NicknameResolverAssemblyLocation = ReadConfigString(nicknameResolverConfigElement, "Assembly", NICKNAME_RESOLVER_ASSEMBLY, xmlConfigurationFile);
                result.NicknameResolverConfigElement = nicknameResolverConfigElement.Element("Config");
            }

            return result;
        }

        #endregion
    }
}