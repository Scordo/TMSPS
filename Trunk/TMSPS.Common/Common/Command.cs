using System;
using System.Collections.Generic;
using System.Reflection;

namespace TMSPS.Core.Common
{
    public enum Command
    {
        [CommandName("Kick"), CommandRight("Kick"), CommandOptions(RequiresPrefix = true)]
        Kick,
        [CommandName("Warn"), CommandRight("Warn"), CommandOptions(RequiresPrefix = true)]
        Warn,
        [CommandName("Ban"), CommandRight("Ban"), CommandOptions(RequiresPrefix = true)]
        Ban,
        [CommandName("Unban"), CommandRight("Unban"), CommandOptions(RequiresPrefix = true)]
        Unban,
        [CommandName("Blacklist"), CommandRight("Blacklist"), CommandOptions(RequiresPrefix = true)]
        Blacklist,
        [CommandName("Unblacklist"), CommandRight("Unblacklist"), CommandOptions(RequiresPrefix = true)]
        Unblacklist,
        [CommandName("Ignore"), CommandRight("Ignore"), CommandOptions(RequiresPrefix = true)]
        Ignore,
        [CommandName("Unignore"), CommandRight("Unignore"), CommandOptions(RequiresPrefix = true)]
        Unignore,
        [CommandName("AddGuest"), CommandRight("AddGuest"), CommandOptions(RequiresPrefix = true)]
        AddGuest,
        [CommandName("RemoveGuest"), CommandRight("RemoveGuest"), CommandOptions(RequiresPrefix = true)]
        RemoveGuest,
        [CommandName("ForceSpectator"), CommandRight("ForceSpectator"), CommandOptions(RequiresPrefix = true)]
        ForceSpectator,
        [CommandName("WriteTrackList"), CommandRight("WriteTrackList"), CommandOptions(RequiresPrefix = true)]
        WriteTrackList,
        [CommandName("ReadtrackList"), CommandRight("ReadtrackList"), CommandOptions(RequiresPrefix = true)]
        ReadTrackList,
        [CommandName("RemoveTrack"), CommandRight("RemoveTrack"), CommandOptions(RequiresPrefix = true)]
        RemoveCurrentTrack,
        [CommandName("ReadCredentials"), CommandRight("ReadCredentials"), CommandOptions(RequiresPrefix = true)]
        ReadCredentials,
        [CommandName("GetSpectators", "Specs"), CommandRight("GetSpectators", "Specs"), CommandOptions(RequiresPrefix = true)]
        GetSpectators,
        [CommandName("GetMySpectators", "MySpecs"), CommandRight("GetMySpectators", "MySpecs"), CommandOptions(RequiresPrefix = true)]
        GetMySpectators,
        [CommandName("KickSpectators", "KickSpecs"), CommandRight("KickSpectators", "KickSpecs"), CommandOptions(RequiresPrefix = true)]
        KickSpectators,
        [CommandName("KickMySpectators", "KickMySpecs"), CommandRight("KickMySpectators", "KickMySpecs"), CommandOptions(RequiresPrefix = true)]
        KickMySpectators,
        [CommandName("ReadClanTagSettings"), CommandRight("ReadClanTagSettings"), CommandOptions(RequiresPrefix = true)]
        ReadClanTagSettings,
        [CommandName("ReadChatBotSettings"), CommandRight("ReadChatBotSettings"), CommandOptions(RequiresPrefix = true)]
        ReadChatBotSettings,
        [CommandName("GetLocalLogins"), CommandRight("GetLocalLogins"), CommandOptions(RequiresPrefix = true)]
        GetLocalLogins,
        [CommandName("RestartTrack"), CommandRight("RestartTrack"), CommandOptions(RequiresPrefix = true)]
        RestartTrack,
        [CommandName("InsertTrack"), CommandRight("InsertTrack", "Addtrack"), CommandOptions(RequiresPrefix = true)]
        InsertTrack,
        [CommandName("AddTrack"), CommandRight("InsertTrack", "Addtrack"), CommandOptions(RequiresPrefix = true)]
        AddTrack,
        [CommandName("TMXInfo"), CommandOptions(RequiresPrefix = true)]
        TMXInfo,
        [CommandName("DeleteCheater", "RemoveCheater"), CommandRight("DeleteCheater", "RemoveCheater"), CommandOptions(RequiresPrefix = true)]
        DeleteCheater,
        [CommandName("SelectUndrivenTracks"), CommandRight("SelectUndrivenTracks"), CommandOptions(RequiresPrefix = true)]
        SelectUndrivenTracks,
        [CommandName("NextTrack"), CommandRight("NextTrack"), CommandOptions(RequiresPrefix = true)]
        NextTrack,
        [CommandName("Seen", "LastSeen")]
        LastSeen,
        [CommandName("TopSums", "Summary", "TopRaceResults")]
        TopSums,
        [CommandName("TopRanks", "Ranks")]
        TopRanks,
        [CommandName("TopWins")]
        TopWins,
        [CommandName("Info", "Wins", "Played", "Visit")]
        Info,
        [CommandName("SR", "Rank")]
        Rank,
        [CommandName("NSR", "NextRank")]
        NextRank,
        [CommandName("Donate")]
        Donate,
        [CommandName("pm", "wisper", "w")]
        Wisper,
        [CommandName("Help", "Hilfe")]
        Help,
        [CommandName("CreateBattle")]
        CreateBattle,
        [CommandName("JoinBattle")]
        JoinBattle,
        [CommandName("LeaveBattle")]
        LeaveBattle,
        [CommandName("StartBattle")]
        StartBattle,
        [CommandName("StopBattle")]
        StopBattle,
    }

    public static class CommandExtensions
    {
        #region Public Methods

        public static HashSet<string> GetRights(this Command command)
        {
            FieldInfo fieldInfo = typeof (Command).GetField(command.ToString(), BindingFlags.Static | BindingFlags.Public);

            HashSet<string> result = new HashSet<string>();
            
            foreach (CommandRightAttribute rightAttribute in fieldInfo.GetCustomAttributes(typeof(CommandRightAttribute), false))
            {
                result.UnionWith(rightAttribute.CommandRights);
            }

            return result;
        }

        public static HashSet<string> GetCommandNames(this Command command)
        {
            FieldInfo fieldInfo = typeof(Command).GetField(command.ToString(), BindingFlags.Static | BindingFlags.Public);

            HashSet<string> result = new HashSet<string>();

            foreach (CommandNameAttribute nameAttribute in fieldInfo.GetCustomAttributes(typeof(CommandNameAttribute), false))
            {
                result.UnionWith(nameAttribute.CommandNames);
            }

            return result;
        }

        public static CommandOptionsAttribute GetCommandOptions(this Command command)
        {
            FieldInfo fieldInfo = typeof(Command).GetField(command.ToString(), BindingFlags.Static | BindingFlags.Public);

            CommandOptionsAttribute result = new CommandOptionsAttribute();

            foreach (CommandOptionsAttribute optionsAttribute in fieldInfo.GetCustomAttributes(typeof(CommandOptionsAttribute), false))
            {
                result = optionsAttribute.Clone();
                break;
            }

            return result;
        }

        public static CommandInfo ToCommandInfo(this Command command)
        {
            return CommandInfo.Parse(command);
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CommandNameAttribute : Attribute
    {
        #region Properties

        public HashSet<string> CommandNames { get; private set; }

        #endregion

        #region Constructor

        public CommandNameAttribute(params string[] commandNames)
        {
            if (commandNames == null || commandNames.Length == 0)
                throw new ArgumentOutOfRangeException("commandNames", "Please provide at least 1 command name");

            CommandNames = new HashSet<string>();
            Array.ForEach(commandNames, c => CommandNames.Add(c.ToLower()));
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CommandRightAttribute : Attribute
    {
        #region Properties

        public HashSet<string> CommandRights { get; private set; }

        #endregion

        #region Constructor

        public CommandRightAttribute(params string[] commandRights)
        {
            if (commandRights == null || commandRights.Length == 0)
                throw new ArgumentOutOfRangeException("commandRights", "Please provide at least 1 command right");

            CommandRights = new HashSet<string>();
            Array.ForEach(commandRights, c => CommandRights.Add(c.ToLower()));
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CommandOptionsAttribute : Attribute
    {
        #region Properties

        public bool RequiresPrefix { get; set; }

        #endregion

        #region Constructor

        public CommandOptionsAttribute()
        {
            RequiresPrefix = false;
        }

        #endregion

        #region Public Methods

        public CommandOptionsAttribute Clone()
        {
            return new CommandOptionsAttribute { RequiresPrefix = RequiresPrefix };
        }

        #endregion

    }
}