using System;
using System.Linq;

namespace TMSPS.Core.Common
{
    public class CommandHelp
    {
        #region Properties

        public string CommandName { get; private set; }
        public string[] AlternativeCommandNames { get; private set; }
        public string[] NecessaryRights { get; private set; }
        public string Description { get; private set; }
        public string Usage { get; private set; }
        public string UsageExample { get; private set; }

        #endregion

        #region Consturtcors

        public CommandHelp()
        {
            
        }

        public CommandHelp(Command command, string description) : this(command, description, null)
        {

        }

        public CommandHelp(Command command, string description, string usage) : this(command, description, usage, null)
        {

        }

        public CommandHelp(Command command, string description, string usage, string usageExample)
        {
            if (description == null)
                throw new ArgumentNullException("description");

            CommandInfo commandInfo = command.ToCommandInfo();
            InitCommandHelp(commandInfo.CommandNames.ToArray(), description, usage, usageExample, commandInfo.Rights.ToArray());
        }

        public CommandHelp(string[] commandNames, string description, string usage, string usageExample, string necessaryRight)
        {
            InitCommandHelp(commandNames, description, usage, usageExample, necessaryRight);
        }

        #endregion

        #region Non Public Methods

        private void InitCommandHelp(string[] commandNames, string description, string usage, string usageExample, params string[] necessaryRights)
        {
            if (commandNames == null)
                throw new ArgumentNullException("commandNames");

            if (commandNames.Length == 0)
                throw new ArgumentOutOfRangeException("commandNames");

            if (description == null)
                throw new ArgumentNullException("description");

            if (!Array.TrueForAll(commandNames,c => c != null))
                throw new ArgumentException("commandNames contains null values", "commandNames");

            CommandName = commandNames[0];

            AlternativeCommandNames = commandNames.Skip(1).ToArray();
            Description = description;
            Usage = usage;
            UsageExample = usageExample;
            NecessaryRights = necessaryRights;
        }

        #endregion
    }
}