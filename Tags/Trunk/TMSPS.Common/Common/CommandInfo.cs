using System;
using System.Reflection;
using System.Collections.Generic;

namespace TMSPS.Core.Common
{
    public class CommandInfo
    {
        #region Properties

        public CommandOptionsAttribute Options { get; set; }
        public HashSet<string> Rights { get; set; }
        public HashSet<string> CommandNames { get; set; }
        public static ReadOnlyDictionary<Command, CommandInfo> StaticCommandInfoDictionary { get; private set; }

        #endregion

        #region Constructors

        static CommandInfo()
        {
            StaticCommandInfoDictionary = new ReadOnlyDictionary<Command, CommandInfo>(ParseAllCommandsAsDictionary());
        }

        public CommandInfo()
        {
            Options = new CommandOptionsAttribute();
            Rights = new HashSet<string>();
            CommandNames = new HashSet<string>();
        }

        public CommandInfo(Command command)
        {
            Assign(StaticCommandInfoDictionary[command]);
        }

        public CommandInfo(ICustomAttributeProvider fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException("fieldInfo");

            Options = new CommandOptionsAttribute();

            foreach (CommandOptionsAttribute optionsAttribute in fieldInfo.GetCustomAttributes(typeof(CommandOptionsAttribute), false))
            {
                Options = optionsAttribute.Clone();
                break;
            }

            CommandNames = new HashSet<string>();

            foreach (CommandNameAttribute nameAttribute in fieldInfo.GetCustomAttributes(typeof(CommandNameAttribute), false))
            {
                CommandNames.UnionWith(nameAttribute.CommandNames);
            }

            Rights = new HashSet<string>();

            foreach (CommandRightAttribute rightAttribute in fieldInfo.GetCustomAttributes(typeof(CommandRightAttribute), false))
            {
                Rights.UnionWith(rightAttribute.CommandRights);
            }
        }

        #endregion

        #region Public Methods

        public void Assign(CommandInfo commandInfo)
        {
            if (commandInfo == null)
                throw new ArgumentNullException("commandInfo");

            Options = commandInfo.Options.Clone();
            Rights = new HashSet<string>(commandInfo.Rights);
            CommandNames = new HashSet<string>(commandInfo.CommandNames);
        }

        public static CommandInfo Parse(ICustomAttributeProvider fieldInfo)
        {
            return new CommandInfo(fieldInfo);
        }

        public static CommandInfo Parse(Command command)
        {
            return new CommandInfo(command);
        }

        public static IEnumerable<CommandInfo> ParseList(params ICustomAttributeProvider[] fieldInfoList)
        {
            if (fieldInfoList == null)
                yield break;

            foreach (ICustomAttributeProvider provider in fieldInfoList)
            {
                yield return new CommandInfo(provider);
            }
        }

        public static IEnumerable<CommandInfo> ParseList(params Command[] commandList)
        {
            if (commandList == null)
                yield break;

            foreach (Command command in commandList)
            {
                yield return new CommandInfo(command);
            }
        }

        public static IEnumerable<CommandInfo> ParseAllCommands()
        {
            foreach (FieldInfo fieldInfo in typeof(Command).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                yield return Parse(fieldInfo);
            }
        }

        public static Dictionary<Command, CommandInfo> ParseAllCommandsAsDictionary()
        {
            Dictionary<Command, CommandInfo> result = new Dictionary<Command, CommandInfo>();

            foreach (FieldInfo fieldInfo in typeof(Command).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                Command command = (Command) fieldInfo.GetValue(null);
                result.Add(command, Parse(fieldInfo));
            }

            return result;
        }

        public bool ContaisCommandName(string commandName)
        {
            return commandName != null && CommandNames.Contains(commandName.ToLower());
        }

        #endregion
    }
}