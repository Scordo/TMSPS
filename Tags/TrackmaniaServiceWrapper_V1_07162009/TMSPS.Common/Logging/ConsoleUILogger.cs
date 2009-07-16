using System;
using log4net;

namespace TMSPS.Core.Logging
{
	public class ConsoleUILogger : IUILogger
	{
		#region Members

		private readonly ILog _log;
		private readonly string _messagePrefix;

		#endregion

		#region Properties

		public static IUILogger UniqueInstance
		{
			get; private set;
		}

		#endregion

		#region Constructors

		static ConsoleUILogger()
		{
			UniqueInstance = new ConsoleUILogger("TMSPS", "CORE");
		}

		public ConsoleUILogger()
		{
			_log = LogManager.GetLogger(typeof(ConsoleUILogger));
		}

		public ConsoleUILogger(string name) : this(name, null)
		{

		}

		public ConsoleUILogger(string name, string messagePrefix)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (name.Trim().Length == 0)
				throw new ArgumentException("name is empty", "name");

			_messagePrefix = messagePrefix ?? string.Empty;
			_log = LogManager.GetLogger(name);
		}

		#endregion

		#region IUILogger Members

		void ILogger.Debug(string message)
		{
			((IUILogger) this).Debug(message, false);
		}

		void IUILogger.Debug(string message, bool logToUI)
		{
			((IUILogger)this).Debug(message, null, false);
		}

		void ILogger.Debug(string message, Exception exception)
		{
			((IUILogger)this).Debug(message, exception, false);
		}

		void IUILogger.Debug(string message, Exception exception, bool logToUI)
		{
			((IUILogger)this).Log(LogMode.Debug, message, exception, logToUI);
		}

		void IUILogger.DebugToUI(string message)
		{
			((IUILogger)this).DebugToUI(message, null);
		}

		void IUILogger.DebugToUI(string message, Exception exception)
		{
			((IUILogger)this).Debug(message, null, true);
		}

		void ILogger.Info(string message)
		{
			((IUILogger)this).Info(message, false);
		}

		void IUILogger.Info(string message, bool logToUI)
		{
			((IUILogger)this).Info(message, null, logToUI);
		}

		void ILogger.Info(string message, Exception exception)
		{
			((IUILogger)this).Info(message, exception, false);
		}

		void IUILogger.Info(string message, Exception exception, bool logToUI)
		{
			((IUILogger)this).Log(LogMode.Info, message, exception, logToUI);
		}

		void IUILogger.InfoToUI(string message)
		{
			((IUILogger)this).InfoToUI(message, null);
		}

		void IUILogger.InfoToUI(string message, Exception exception)
		{
			((IUILogger)this).Info(message, exception, true);
		}

		void ILogger.Warn(string message)
		{
			((IUILogger)this).Warn(message, false);
		}

		void IUILogger.Warn(string message, bool logToUI)
		{
			((IUILogger)this).Warn(message, null, logToUI);
		}

		void ILogger.Warn(string message, Exception exception)
		{
			((IUILogger)this).Warn(message, exception, false);
		}

		void IUILogger.Warn(string message, Exception exception, bool logToUI)
		{
			((IUILogger)this).Log(LogMode.Warn, message, exception, logToUI);
		}

		void IUILogger.WarnToUI(string message)
		{
			((IUILogger)this).WarnToUI(message, null);
		}

		void IUILogger.WarnToUI(string message, Exception exception)
		{
			((IUILogger)this).Warn(message, exception, true);
		}

		void ILogger.Error(string message)
		{
			((IUILogger)this).Error(message, false);
		}

		void IUILogger.Error(string message, bool logToUI)
		{
			((IUILogger)this).Error(message, null, logToUI);
		}

		void ILogger.Error(string message, Exception exception)
		{
			((IUILogger)this).Error(message, exception, false);
		}

		void IUILogger.Error(string message, Exception exception, bool logToUI)
		{
			((IUILogger)this).Log(LogMode.Error, message, exception, logToUI);
		}

		void IUILogger.ErrorToUI(string message)
		{
			((IUILogger)this).ErrorToUI(message, null);
		}

		void IUILogger.ErrorToUI(string message, Exception exception)
		{
			((IUILogger)this).Error(message, exception, true);
		}

		void ILogger.Fatal(string message)
		{
			((IUILogger)this).Fatal(message, false);
		}

		void IUILogger.Fatal(string message, bool logToUI)
		{
			((IUILogger)this).Fatal(message, null, logToUI);
		}

		void ILogger.Fatal(string message, Exception exception)
		{
			((IUILogger)this).Fatal(message, exception, false);
		}

		void IUILogger.Fatal(string message, Exception exception, bool logToUI)
		{
			((IUILogger)this).Log(LogMode.Fatal, message, exception, logToUI);
		}

		void IUILogger.FatalToUI(string message)
		{
			((IUILogger)this).FatalToUI(message, null);
		}

		void IUILogger.FatalToUI(string message, Exception exception)
		{
			((IUILogger)this).Fatal(message, exception, true);
		}

		void ILogger.Log(LogMode logMode, string message)
		{
			((IUILogger)this).Log(logMode, message, null);
		}

		void ILogger.Log(LogMode logMode, string message, Exception exception)
		{
			((IUILogger)this).Log(logMode, message, exception, false);
		}

		void IUILogger.Log(LogMode logMode, string message, bool logToUI)
		{
			((IUILogger)this).Log(logMode, message, null, logToUI);
		}

		void IUILogger.Log(LogMode logMode, string message, Exception exception, bool logToUI)
		{
			string logModePrefix = string.Format("[{0}]", logMode.ToString().ToUpper());
			string exceptionMessage = string.Empty;

			if (exception != null)
				exceptionMessage = string.Concat(" - [Exception: ", exception, "]");

			switch (logMode)
			{
				case LogMode.Debug:
					_log.Debug(_messagePrefix + message, exception);
					break;
				case LogMode.Info:
					_log.Info(_messagePrefix + message, exception);
					break;
				case LogMode.Warn:
					_log.Warn(_messagePrefix + message, exception);
					break;
				case LogMode.Error:
					_log.Error(_messagePrefix + message, exception);
					break;
				case LogMode.Fatal:
					_log.Fatal(_messagePrefix + message, exception);
					break;
			}

			if (logToUI)
				Console.WriteLine(string.Concat(logModePrefix, _messagePrefix, " ", message, exceptionMessage));
		}

		void IUILogger.LogToUI(LogMode logMode, string message)
		{
			((IUILogger)this).LogToUI(logMode, message, null);
		}

		void IUILogger.LogToUI(LogMode logMode, string message, Exception exception)
		{
			((IUILogger)this).Log(logMode, message, exception, true);
		}

		#endregion

	}
}