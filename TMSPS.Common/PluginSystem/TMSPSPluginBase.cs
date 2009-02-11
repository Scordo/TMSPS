using System;
using System.IO;
using System.Reflection;
using TMSPS.Core.Common;
using TMSPS.Core.Logging;

namespace TMSPS.Core.PluginSystem
{
    public abstract class TMSPSPluginBase : ITMSPSPlugin
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
    	protected static  string ApplicationDirectory { get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); } }
        protected static string PluginsDirectory { get { return Path.Combine(ApplicationDirectory, "Plugins"); } }
        protected string PluginDirectory { get { return Path.Combine(PluginsDirectory, ShortName); } }
        protected string PluginSettingsFilePath { get { return Path.Combine(PluginDirectory, "Settings.xml"); } }

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

    	public void DisposePlugin()
    	{
			RunCatchLogReThrow(Dispose, "Error during disposing plugin.", true);
    	}

    	#endregion

    	#region Non Public Methods

    	protected abstract void Init();
    	protected abstract void Dispose();

    	protected virtual string GetPluginConfigFilePath(string configFileName)
    	{
    		return Path.Combine(PluginDirectory, configFileName);
    	}

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

		private static void RunCatchLogThrow(ParameterlessMethodDelegate logic, string additionalMessage, bool msgToUI, bool rethrowException, IUILogger logger)
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

    	#endregion
    }
}