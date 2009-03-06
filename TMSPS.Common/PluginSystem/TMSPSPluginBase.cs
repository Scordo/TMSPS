using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TMSPS.Core.Common;
using TMSPS.Core.Logging;

namespace TMSPS.Core.PluginSystem
{
	public  abstract partial class TMSPSPluginBase : ITMSPSPluginBase
	{
		#region Members

		private PluginHostContext _context;

		#endregion

		#region Properties

		public abstract Version Version { get; }
		public abstract string Author { get; }
		public abstract string Name { get; }
		public abstract string Description { get; }
		public abstract string ShortName { get; }
		public IUILogger Logger { get; private set; }
		public PluginHostContext Context { get { return _context; } }
		public static string ApplicationDirectory { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }
        public static string PluginsDirectory { get { return Path.Combine(ApplicationDirectory, "Plugins"); } }
        public virtual string PluginDirectory { get { return Path.Combine(PluginsDirectory, ShortName); } }
        public string PluginSettingsFilePath { get { return Path.Combine(PluginDirectory, "Settings.xml"); } }

		#endregion

		#region Public Methods

		public void InitPlugin(PluginHostContext context, IUILogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");

			Logger = logger;
			_context = context;

			RunCatchLogReThrow(Init, "Error initializing plugin.", true);
		}

        public void DisposePlugin(bool connectionLost)
		{
			RunCatchLogReThrow(() => Dispose(connectionLost), "Error during disposing plugin.", true);
		}

		#endregion

		#region Non Public Methods

		protected abstract void Init();
        protected abstract void Dispose(bool connectionLost);

		protected void RunCatchLog(ParameterlessMethodDelegate logic)
		{
			RunCatchLog(logic, null, false);
		}

		protected static void RunCatchLog(ParameterlessMethodDelegate logic, IUILogger logger)
		{
			RunCatchLog(logic, null, false, logger);
		}

		protected void RunCatchLog(ParameterlessMethodDelegate logic, string additionalMessage)
		{
			RunCatchLog(logic, additionalMessage, false);
		}

		protected static void RunCatchLog(ParameterlessMethodDelegate logic, string additionalMessage, IUILogger logger)
		{
			RunCatchLog(logic, additionalMessage, false, logger);
		}

		protected void RunCatchLog(ParameterlessMethodDelegate logic, string additionalMessage, bool msgToUI)
		{
			RunCatchLogThrow(logic, additionalMessage, msgToUI, false);
		}

		protected static void RunCatchLog(ParameterlessMethodDelegate logic, string additionalMessage, bool msgToUI, IUILogger logger)
		{
			RunCatchLogThrow(logic, additionalMessage, msgToUI, false, logger);
		}

		protected void RunCatchLogReThrow(ParameterlessMethodDelegate logic)
		{
			RunCatchLogReThrow(logic, null, false);
		}

		protected static void RunCatchLogReThrow(ParameterlessMethodDelegate logic, IUILogger logger)
		{
			RunCatchLogReThrow(logic, null, false, logger);
		}

		protected void RunCatchLogReThrow(ParameterlessMethodDelegate logic, string additionalMessage)
		{
			RunCatchLogReThrow(logic, additionalMessage, false);
		}

		protected static void RunCatchLogReThrow(ParameterlessMethodDelegate logic, string additionalMessage, IUILogger logger)
		{
			RunCatchLogReThrow(logic, additionalMessage, false, logger);
		}

		protected void RunCatchLogReThrow(ParameterlessMethodDelegate logic, string additionalMessage, bool msgToUI)
		{
			RunCatchLogThrow(logic, additionalMessage, msgToUI, true);
		}

		protected static void RunCatchLogReThrow(ParameterlessMethodDelegate logic, string additionalMessage, bool msgToUI, IUILogger logger)
		{
			RunCatchLogThrow(logic, additionalMessage, msgToUI, true, logger);
		}

		private void RunCatchLogThrow(ParameterlessMethodDelegate logic, string additionalMessage, bool msgToUI, bool rethrowException)
		{
			RunCatchLogThrow(logic, additionalMessage, msgToUI, rethrowException, Logger);
		}

		protected static void RunCatchLogThrow(ParameterlessMethodDelegate logic, string additionalMessage, bool msgToUI, bool rethrowException, IUILogger logger)
		{
			if (logger == null)
				throw new ArgumentNullException("logger");

			try
			{
				logic();
			}
			catch (Exception ex)
			{
				if (msgToUI && !additionalMessage.IsNullOrTimmedEmpty())
					logger.ErrorToUI(additionalMessage);

				logger.Error(additionalMessage, ex);

				if (rethrowException)
					throw;
			}
		}

        protected static bool CheckpointsValid(IEnumerable<int> checkpoints)
        {
            int lastCheckpoint = 0;

            foreach (int checkpoint in checkpoints)
            {
                if (checkpoint <= lastCheckpoint)
                    return false;

                lastCheckpoint = checkpoint;
            }

            return true;
        }

        protected void SendNoPermissionMessagetoLogin(string login)
        {
            SendFormattedMessage(login, "{[#ServerStyle]}> {[#ErrorStyle]}You do not have permissions to execute this command.");
        }

        protected bool LoginHasRight(string login, bool sendNoPermissionMessageIfRightMissing, string right)
        {
            return LoginHasAnyRight(login, sendNoPermissionMessageIfRightMissing, right);
        }

        protected bool LoginHasAnyRight(string login, bool sendNoPermissionMessageIfRightMissing, params string[] rights)
        {
            if (!Context.Credentials.UserHasAnyRight(login, rights))
            {
                if (sendNoPermissionMessageIfRightMissing)
                    SendNoPermissionMessagetoLogin(login);

                return false;
            }

            return true;
        }

        public static string StripTMColorsAndFormatting(string input)
        {
            if (input.IsNullOrTimmedEmpty())
                return input;

            input = input.Replace("$$", "\t");
            input = Regex.Replace(input, @"\$[\da-f]{3}", string.Empty, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"\$[istwnmgzhl]{1}", string.Empty, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return input.Replace("\t", "$$");
        }

        public static string StripTMColors(string input)
        {
            if (input.IsNullOrTimmedEmpty())
                return input;

            input = input.Replace("$$", "\t");
            input = Regex.Replace(input, @"\$[\da-f]{3}", string.Empty, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            return input.Replace("\t", "$$");
        }

        public static string StripTMFormatting(string input)
        {
            if (input.IsNullOrTimmedEmpty())
                return input;

            input = input.Replace("$$", "\t");
            input = Regex.Replace(input, @"\$[istwnmgzhl]{1}", string.Empty, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return input.Replace("\t", "$$");
        }

        protected string ReplaceMessageConstants(string message)
        {
            return ReplaceMessagePlaceHolders(message, Context.MessageConstants.ToArray());
        }

        protected string ReplaceMessageStyles(string message)
        {
            return Context.MessagStyles.ReplaceStyles(message);
        }

        protected string FormatMessage(string message, params string[] keyValuePairs)
        {
            return ReplaceMessagePlaceHolders(ReplaceMessageStyles(ReplaceMessageConstants(message)), keyValuePairs);
        }

        protected void SendFormattedMessageToLogin(string login, string message, params string[] keyValuePairs)
        {
            Context.RPCClient.Methods.ChatSendServerMessageToLogin(FormatMessage(message, keyValuePairs), login);
        }

        protected void SendFormattedMessage(string message, params string[] keyValuePairs)
        {
            Context.RPCClient.Methods.ChatSendServerMessage(FormatMessage(message, keyValuePairs));
        }

        public static string ReplaceMessagePlaceHolders(string message, params string[] keyValuePairs)
        {
            if (message.IsNullOrTimmedEmpty() || keyValuePairs == null || keyValuePairs.Length == 0)
                return message;

            if (keyValuePairs.Length % 2 != 0)
                throw new ArgumentOutOfRangeException("keyValuePairs", "The amount of passed strings must be even!");

            return Regex.Replace(message, @"{\[(?<tag>[^\[{#}\]]+)\]}", match =>
            {
                string tag = match.Groups["tag"].Value.ToLower(CultureInfo.InvariantCulture);

                for (int i = 0; i < keyValuePairs.Length; i += 2)
                {
                    if (string.Compare(tag, keyValuePairs[i], StringComparison.InvariantCultureIgnoreCase) == 0)
                        return keyValuePairs[i + 1];
                }

                return match.Value;
            }, RegexOptions.Singleline | RegexOptions.Compiled);
        }

		#endregion
	}
}
