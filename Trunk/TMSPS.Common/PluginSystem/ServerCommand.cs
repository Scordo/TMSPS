using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace TMSPS.Core.PluginSystem
{
    public class ServerCommand
    {
        #region Properties

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

            bool isCommand = Array.Exists(ValidCommandPrefixes, command => chatMessage.StartsWith(string.Format("/{0} ", command), StringComparison.InvariantCultureIgnoreCase));

            if (!isCommand)
                return null;

            const string commandPattern = @"/\w+\s+(?<Command>.*)";

            Match match = Regex.Match(chatMessage, commandPattern, RegexOptions.Compiled | RegexOptions.Singleline);

            if (!match.Success)
                return null;

            string[] commandParts = match.Groups["Command"].Value.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            if (commandParts.Length == 0)
                return null;

            ServerCommand result = new ServerCommand
            {
                OriginalText = chatMessage,
                CommandText = match.Groups["Command"].Value,
                MainCommand = commandParts[0]
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
    }

    public static class ServerCommandExtensions
    {
        public static bool IsMainCommandAnyOf(this ServerCommand serverCommand, params string[] commandNames)
        {
            if (serverCommand == null || commandNames == null)
                return false;

            return Array.Exists(commandNames, command => string.Compare(serverCommand.MainCommand, command, StringComparison.InvariantCultureIgnoreCase) == 0);
        }
    }
}