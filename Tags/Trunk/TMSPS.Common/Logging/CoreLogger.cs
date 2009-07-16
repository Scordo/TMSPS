using System;
using log4net;

namespace TMSPS.Core.Logging
{
    public class CoreLogger : ILogger
    {
		#region Members

		private readonly ILog _log;
		private readonly string _messagePrefix;

		#endregion

		#region Properties

		public static ILogger UniqueInstance
		{
			get; private set;
		}

		#endregion

		#region Constructors

		static CoreLogger()
		{
			UniqueInstance = new CoreLogger("TMSPS", "CORE");
		}

		public CoreLogger()
		{
			_log = LogManager.GetLogger(typeof(CoreLogger));
		}

		public CoreLogger(string name) : this(name, null)
		{

		}

		public CoreLogger(string name, string messagePrefix)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (name.Trim().Length == 0)
				throw new ArgumentException("name is empty", "name");

			_messagePrefix = messagePrefix ?? string.Empty;
			_log = LogManager.GetLogger(name);
		}

		#endregion

		#region ILogger Members

		void ILogger.Debug(string message)
		{
			((ILogger) this).Debug(message, null);
		}

		void ILogger.Debug(string message, Exception exception)
		{
			((ILogger)this).Log(LogMode.Debug, message, exception);
		}

		void ILogger.Info(string message)
		{
			((ILogger)this).Info(message, null);
		}

		void ILogger.Info(string message, Exception exception)
		{
			((ILogger)this).Log(LogMode.Info, message, exception);
		}

		void ILogger.Warn(string message)
		{
			((ILogger)this).Warn(message, null);
		}

		void ILogger.Warn(string message, Exception exception)
		{
			((ILogger)this).Log(LogMode.Warn, message, exception);
		}

		void ILogger.Error(string message)
		{
			((ILogger)this).Error(message, null);
		}

		void ILogger.Error(string message, Exception exception)
		{
			((ILogger)this).Log(LogMode.Error, message, exception);
		}

		void ILogger.Fatal(string message)
		{
			((ILogger)this).Fatal(message, null);
		}

		void ILogger.Fatal(string message, Exception exception)
		{
			((ILogger)this).Log(LogMode.Fatal, message, exception);
		}

		void ILogger.Log(LogMode logMode, string message)
		{
			((ILogger)this).Log(logMode, message, null);
		}

		void ILogger.Log(LogMode logMode, string message, Exception exception)
		{
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
		}

		#endregion
	}
}
