using System;

namespace TMSPS.Core.Logging
{
	

	public interface IUILogger : ILogger
	{
		void Debug(string message, bool logToUI);
		void Debug(string message, Exception exception, bool logToUI);
		void DebugToUI(string message);
		void DebugToUI(string message, Exception exception);

		void Info(string message, bool logToUI);
		void Info(string message, Exception exception, bool logToUI);
		void InfoToUI(string message);
		void InfoToUI(string message, Exception exception);

		void Warn(string message, bool logToUI);
		void Warn(string message, Exception exception, bool logToUI);
		void WarnToUI(string message);
		void WarnToUI(string message, Exception exception);

		void Error(string message, bool logToUI);
		void Error(string message, Exception exception, bool logToUI);
		void ErrorToUI(string message);
		void ErrorToUI(string message, Exception exception);

		void Fatal(string message, bool logToUI);
		void Fatal(string message, Exception exception, bool logToUI);
		void FatalToUI(string message);
		void FatalToUI(string message, Exception exception);

		void Log(LogMode logMode, string message, bool logToUI);
		void Log(LogMode logMode, string message, Exception exception, bool logToUI);

		void LogToUI(LogMode logMode, string message);
		void LogToUI(LogMode logMode, string message, Exception exception);
	}
}
