using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
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

            RunPlatformDependent(Environment.OSVersion.Platform);
        }

        private static void RunPlatformDependent(PlatformID platform)
        {
            switch (platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32Windows:
                case PlatformID.Win32S:
                    if (Environment.UserInteractive)
                        RunInTerminal();
                    else
                        RunAsWindowsService();
                    break;
                case PlatformID.Unix:
                    RunInTerminal();
                    break;
                default:
                    Console.WriteLine("Your System is not supported: " + platform);
                    break;
            }
        }

        private static void RunAsWindowsService()
        {
            int startUpIdleTime;
            int.TryParse(ConfigurationManager.AppSettings["StartupIdle"], out startUpIdleTime);
            _log.Debug("Running in service mode.");
            Thread.Sleep(startUpIdleTime);
            ServiceBase.Run(new[] { new MainService() });
        }

        private static void RunInTerminal()
        {
            Console.WriteLine("Running in terminal mode.");
            _log.Debug("Running in terminal mode.");

            _mainDaemon = new MainDaemon();
            _mainDaemon.Start();
            Console.WriteLine("Press Enter to stop the service...");
            Console.ReadLine();
            _mainDaemon.Stop();

            _log.Debug("Finished in terminal mode.");
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.ErrorFormat("Unhandled error: {0}", e.ExceptionObject);
        }
    }
}