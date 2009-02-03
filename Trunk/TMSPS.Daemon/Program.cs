using System;
using System.ServiceProcess;
using log4net;
using log4net.Config;

namespace TMSPS.Daemon
{
    static class Program
    {
        #region Non Public Members

        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));
        private static MainDaemon _mainDaemon;

        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			if (args.Length > 0 && (args[0] == "-s" || args[0] == "-service"))
            {
				_log.Debug("Running in service mode.");
				ServiceBase.Run(new[] { new MainService() });
            }
            else
            {
				Console.WriteLine("Running in console mode.");
				_log.Debug("Running in console mode.");

				_mainDaemon = new MainDaemon();
				_mainDaemon.Start();
				Console.WriteLine("Press Enter to stop the service...");
				Console.ReadLine();
				_mainDaemon.Stop();

				_log.Debug("Finished in console mode.");
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.ErrorFormat("Unhandled error: {0}", e.ExceptionObject);
        }
    }
}
