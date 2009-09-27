using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.Logging;
using System.Linq;
using TMSPS.Core.ManiaLinking;
using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.Core.PluginSystem
{
	public abstract partial class TMSPSPluginBase : ITMSPSPluginBase
	{
		#region Members

		private PluginHostContext _context;
        private ushort _pluginID;
        private static readonly char[] _knownFormattingChars =  new[]       {'i', 's', 't', 'w', 'n', 'm', 'g', 'z', 'h', 'l', 'p',
                                                                             'I', 'S', 'T', 'W', 'N', 'M', 'G', 'Z', 'H', 'L', 'P'};

        private static readonly char[] _knownColorAndFormattingChars = new[]{'i', 's', 't', 'w', 'n', 'm', 'g', 'z', 'h', 'l', 'p',
                                                                             'I', 'S', 'T', 'W', 'N', 'M', 'G', 'Z', 'H', 'L', 'P',
                                                                             '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
                                                                             'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F'};

		#endregion

		#region Properties

	    public ushort ID { get { return _pluginID; } }
		public abstract Version Version { get; }
		public abstract string Author { get; }
		public abstract string Name { get; }
		public abstract string Description { get; }
		public abstract string ShortName { get; }
		public IUILogger Logger { get; private set; }
		public PluginHostContext Context { get { return _context; } }
		public static string ApplicationDirectory { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }
        public static string PluginsDirectory { get { return Path.Combine(ApplicationDirectory, "Plugins"); } }
        public virtual string PluginDirectory { get; protected set;  }
        public string PluginSettingsFilePath { get { return Path.Combine(PluginDirectory, "Settings.xml"); } }
        public virtual IEnumerable<CommandHelp> CommandHelpList { get { return null; }}

		#endregion

	    #region Constructor

        protected TMSPSPluginBase(string pluginDirectory)
        {
            if (!Directory.Exists(pluginDirectory))
                throw new DirectoryNotFoundException(pluginDirectory);

            PluginDirectory = pluginDirectory;
        }

	    #endregion


	    #region Public Methods

	    public static List<ITMSPSPlugin> GetPlugins(string mainDirectory, ILogger logger)
	    {
	        string pluginDirectory = Path.Combine(mainDirectory, "Plugins");

            List<ITMSPSPlugin> result = new List<ITMSPSPlugin>();

	        PluginConfigEntryCollection pluginConfigEntries = PluginConfigEntryCollection.ReadFromDirectory(pluginDirectory, logger).GetEnabledPlugins();

	        foreach (PluginConfigEntry pluginConfigEntry in pluginConfigEntries)
	        {
	            logger.Debug(string.Format("Instantiating ITMSPSPlugin {0}", pluginConfigEntry.PluginClass));
	            result.Add(Instancer.GetInstanceOfInterface<ITMSPSPlugin>(pluginConfigEntry.AssemblyName, pluginConfigEntry.PluginClass, pluginConfigEntry.PluginDirectory));
	        }

	        return result;
	    }

	    public void InitPlugin(PluginHostContext context, IUILogger logger)
	    {
	        if (logger == null)
	            throw new ArgumentNullException("logger");

	        Logger = logger;
	        _context = context;
	        _pluginID = Context.GetUniquePluginID();
	        Context.RPCClient.Callbacks.PlayerManialinkPageAnswer += Callbacks_PlayerManialinkPageAnswer;

	        RunCatchLogReThrow(Init, "Error initializing plugin.", true);
	    }

        
	    public void DisposePlugin(bool connectionLost)
	    {
	        Context.RPCClient.Callbacks.PlayerManialinkPageAnswer -= Callbacks_PlayerManialinkPageAnswer;
	        RunCatchLogReThrow(() => Dispose(connectionLost), "Error during disposing plugin.", true);
	    }

	    #endregion

	    #region Non Public Methods

	    protected abstract void Init();
	    protected abstract void Dispose(bool connectionLost);

	    private void Callbacks_PlayerManialinkPageAnswer(object sender, Communication.EventArguments.Callbacks.PlayerManialinkPageAnswerEventArgs e)
	    {
	        if (e.Answer < 0)
	            return;

	        TMAction action = TMAction.Parse(Convert.ToUInt32(e.Answer));
	        if (action == null)
	            return;

	        if (action.PluginID == ID)
	            OnManiaLinkPageAnswer(e.Login, e.PlayerID, action);
	    }

	    protected bool LogFaultResponse<T>(ResponseBase<T> faulResponse, string currentMethodName) where T : ResponseBase<T>
	    {
	        if (!faulResponse.Erroneous)
	            return false;
            
	        Logger.ErrorToUI(string.Format("Error while calling method '{0}': {1}({2})", currentMethodName, faulResponse.Fault.FaultMessage, faulResponse.Fault.FaultCode));
	        return true;
	    }

	    protected virtual void OnManiaLinkPageAnswer(string login, int playerID, TMAction action)
	    {
            
	    }

	    protected void RunCatchLog(Action logic)
	    {
	        RunCatchLog(logic, null, false);
	    }

	    protected static void RunCatchLog(Action logic, IUILogger logger)
	    {
	        RunCatchLog(logic, null, false, logger);
	    }

	    protected void RunCatchLog(Action logic, string additionalMessage)
	    {
	        RunCatchLog(logic, additionalMessage, false);
	    }

	    protected static void RunCatchLog(Action logic, string additionalMessage, IUILogger logger)
	    {
	        RunCatchLog(logic, additionalMessage, false, logger);
	    }

	    protected void RunCatchLog(Action logic, string additionalMessage, bool msgToUI)
	    {
	        RunCatchLogThrow(logic, additionalMessage, msgToUI, false);
	    }

	    protected static void RunCatchLog(Action logic, string additionalMessage, bool msgToUI, IUILogger logger)
	    {
	        RunCatchLogThrow(logic, additionalMessage, msgToUI, false, logger);
	    }

	    protected void RunCatchLogReThrow(Action logic)
	    {
	        RunCatchLogReThrow(logic, null, false);
	    }

	    protected static void RunCatchLogReThrow(Action logic, IUILogger logger)
	    {
	        RunCatchLogReThrow(logic, null, false, logger);
	    }

	    protected void RunCatchLogReThrow(Action logic, string additionalMessage)
	    {
	        RunCatchLogReThrow(logic, additionalMessage, false);
	    }

	    protected static void RunCatchLogReThrow(Action logic, string additionalMessage, IUILogger logger)
	    {
	        RunCatchLogReThrow(logic, additionalMessage, false, logger);
	    }

	    protected void RunCatchLogReThrow(Action logic, string additionalMessage, bool msgToUI)
	    {
	        RunCatchLogThrow(logic, additionalMessage, msgToUI, true);
	    }

	    protected static void RunCatchLogReThrow(Action logic, string additionalMessage, bool msgToUI, IUILogger logger)
	    {
	        RunCatchLogThrow(logic, additionalMessage, msgToUI, true, logger);
	    }

	    private void RunCatchLogThrow(Action logic, string additionalMessage, bool msgToUI, bool rethrowException)
	    {
	        RunCatchLogThrow(logic, additionalMessage, msgToUI, rethrowException, Logger);
	    }

	    protected static void RunCatchLogThrow(Action logic, string additionalMessage, bool msgToUI, bool rethrowException, IUILogger logger)
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
	        // if all checkpoints have a value of -1 return true, otherwise non cheaters get banned 
	        // this case happens when a player connects, drives a time... disconnects within the same round.. connects within the same round.. does not drive a time within the same round .. and disconnects within the same round
	        if (checkpoints.All(time => time == -1))
	            return true;

	        int lastCheckpoint = -1;

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
	        SendFormattedMessageToLogin(login, "{[#ServerStyle]}> {[#ErrorStyle]}You do not have permissions to execute this command.");
	    }

	    protected void SendNoPlayerWithLoginMessageToLogin(string login, string missingLogin)
	    {
	        SendFormattedMessageToLogin(login, Context.CorePlugin.Settings.LoginMissingMessage, "Login", missingLogin);
	    }

	    protected bool LoginHasRight(string login, bool sendNoPermissionMessageIfRightMissing, Command command)
	    {
	        return LoginHasRight(login, sendNoPermissionMessageIfRightMissing, CommandInfo.Parse(command));
	    }

	    protected bool LoginHasRight(string login, bool sendNoPermissionMessageIfRightMissing, CommandInfo commandInfo)
	    {
	        return LoginHasAnyRight(login, sendNoPermissionMessageIfRightMissing, commandInfo.Rights.ToArray());
	    }

	    protected bool LoginHasRight(string login, bool sendNoPermissionMessageIfRightMissing, string right)
	    {
	        return LoginHasAnyRight(login, sendNoPermissionMessageIfRightMissing, right);
	    }

	    protected bool LoginHasAnyRight(string login, bool sendNoPermissionMessageIfRightMissing, params CommandInfo[] CommandInfoList)
	    {
	        return LoginHasAnyRight(login, sendNoPermissionMessageIfRightMissing, CommandInfoList.SelectMany(x => x.Rights).ToArray());
	    }

	    protected bool LoginHasAnyRight(string login, bool sendNoPermissionMessageIfRightMissing, params Command[] commands)
	    {
	        return LoginHasAnyRight(login, sendNoPermissionMessageIfRightMissing, commands.Select(x => CommandInfo.Parse(x)).ToArray());
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

	        string result = string.Empty;
	        bool previousCharIsDollar = false;
	        bool colorOrFormattingPhase = false;
	        short formatCharsCount = 0;

	        foreach (char character in input)
	        {
	            if (character == '$')
	            {
	                colorOrFormattingPhase = false;

	                if (previousCharIsDollar)
	                    result = string.Concat(result, "$$");

	                previousCharIsDollar = !previousCharIsDollar;
	                continue;
	            }

	            if (previousCharIsDollar)
	            {
	                colorOrFormattingPhase = true;
	                formatCharsCount = 0;
	                previousCharIsDollar = false;
	            }

	            if (!colorOrFormattingPhase)
	            {
	                result = string.Concat(result, character);
	                continue;
	            }

	            // strip invalid formatting statements
	            if (formatCharsCount == 0 && Array.IndexOf(_knownColorAndFormattingChars, character) == -1)
	            {
	                colorOrFormattingPhase = false;
	                continue;
	            }

	            // strip formatting statements
	            if (formatCharsCount == 0 && Array.IndexOf(_knownFormattingChars, character) != -1)
	            {
	                colorOrFormattingPhase = false;
	                continue;
	            }

	            // color code is complete 
	            if (formatCharsCount == 2)
	            {
	                colorOrFormattingPhase = false;
	                continue;
	            }

	            formatCharsCount++;
	        }

	        return result;
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

	    protected static uint[] GetPageIndices(ushort pageIndex, uint entriesPerPage)
	    {
	        uint startIndex = Convert.ToUInt32(pageIndex * entriesPerPage);
	        uint endIndex = Convert.ToUInt32(((pageIndex + 1) * entriesPerPage) - 1);

	        return new[] { startIndex, endIndex};
	    }

	    protected PlayerSettings GetPlayerSettings(string login)
	    {
	        return Context.PlayerSettings.Get(login);
	    }

	    protected PlayerSettings GetPlayerSettings(string login, bool createOnDemand)
	    {
	        return Context.PlayerSettings.Get(login, createOnDemand);
	    }

	    protected PluginSettings GetPluginSettings(string login)
	    {
	        return Context.PlayerSettings.Get(login, ID);
	    }

	    public PluginAreaSettings GetAreaSettings(string login, byte areaID)
	    {
	        return Context.PlayerSettings.Get(login, ID, areaID);
	    }

	    public string GetManiaLinkPageHash(string login, string maniaLinkPageID)
	    {
	        PlayerSettings playerSettings = GetPlayerSettings(login);

	        return playerSettings != null ? playerSettings.ManiaLinkPageHashStore.Get(maniaLinkPageID) : null;
	    }

	    public void SetManiaLinkPageHash(string login, string maniaLinkPageID, string hash)
	    {
	        PlayerSettings playerSettings = GetPlayerSettings(login);

	        if (playerSettings != null)
	            playerSettings.ManiaLinkPageHashStore[maniaLinkPageID] = hash;
	    }

	    #endregion
	}
}