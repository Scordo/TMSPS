using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem
{
    public class ServerCommand
    {
        #region Properties

        public bool HasCommandPrefix { get; private set; }
        public string OriginalText { get; private set;}
        public string CommandText { get; private set; }
        public string MainCommand { get; private set; }
        public ReadOnlyCollection<string> Parts { get; private set; }
        public ReadOnlyCollection<string> PartsWithoutMainCommand { get; private set; }
        public static string[] ValidCommandPrefixes {get { return new [] {"tmsps", "t"};} }

        #endregion

        #region Constructors

        private ServerCommand()
        {
            
        }

        #endregion

        #region Public Methods

        public static bool IsServerCommand(string chatMessage)
        {
            return Parse(chatMessage) != null;
        }

        public static ServerCommand Parse(string chatMessage)
        {
            if (chatMessage == null)
                return null;

            if (!chatMessage.StartsWith("/"))
                return null;

            string originalMessage = chatMessage;
            bool hasCommandPrefix = TrimmCommandPrefix(chatMessage, out chatMessage);

            string[] commandParts = chatMessage.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (commandParts.Length == 0)
                return null;

            ServerCommand result = new ServerCommand
            {
                OriginalText = originalMessage,
                CommandText = chatMessage,
                MainCommand = commandParts[0],
                HasCommandPrefix = hasCommandPrefix
            };

            List<string> parts = new List<string>();
            List<string> partsWithoutMainCommand = new List<string>();

            for (int i = 0; i < commandParts.Length; i++ )
            {
                parts.Add(commandParts[i]);

                if (i > 0)
                    partsWithoutMainCommand.Add(commandParts[i]);
            }

            result.Parts = parts.AsReadOnly();
            result.PartsWithoutMainCommand = partsWithoutMainCommand.AsReadOnly();

            return result;
        }

        #endregion

        private static bool TrimmCommandPrefix(string chatMessage, out string modifiedChatMessage)
        {
            foreach (string commandPrefix in ValidCommandPrefixes)
            {
                if (chatMessage.StartsWith(string.Format("/{0} ", commandPrefix), StringComparison.InvariantCultureIgnoreCase))
                {
                    modifiedChatMessage = chatMessage.Substring(commandPrefix.Length+1).Trim();
                    return true;
                }
            }

            modifiedChatMessage = chatMessage.Substring(1);

            return false;
        }
    }

    public static class ServerCommandExtensions
    {
        public static bool IsMainCommandAnyOf(this ServerCommand serverCommand, params string[] commandNames)
        {
            if (serverCommand == null || commandNames == null)
                return false;

            return Array.Exists(commandNames, command => string.Compare(serverCommand.MainCommand, command, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public static bool Is(this ServerCommand serverCommand, Command command)
        {
            return Is(serverCommand, CommandInfo.Parse(command));
        }

        public static bool IsAny(this ServerCommand serverCommand, params CommandInfo[] commandInfoList)
        {
            if (commandInfoList == null || commandInfoList.Length == 0)
                return false;

            foreach (CommandInfo commandInfo in commandInfoList)
            {
                if (serverCommand.Is(commandInfo))
                    return true;
            }

            return false;
        }

        public static bool IsAny(this ServerCommand serverCommand, params Command[] commands)
        {
            if (commands == null || commands.Length == 0)
                return false;

            foreach (Command command in commands)
            {
                if (serverCommand.Is(command))
                    return true;
            }

            return false;
        }

        public static bool Is(this ServerCommand serverCommand, CommandInfo commandInfo)
        {
            if (serverCommand == null || commandInfo == null)
                return false;

            // remove last expression to allow commands requiring a prefix to work withput prefix
            return commandInfo.ContaisCommandName(serverCommand.MainCommand) && commandInfo.Options.RequiresPrefix == serverCommand.HasCommandPrefix;
        }
    }
}