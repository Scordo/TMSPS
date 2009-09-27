using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Install;
using System.Reflection;
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

            if (Environment.UserInteractive)
            {
                if (args != null && args.Length > 0 && (args[0].Equals("/i", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("/install", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("-i", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("-install", StringComparison.InvariantCultureIgnoreCase)))
                {
                    InstallService(args);
                    return;
                }

                if (args != null && args.Length > 0 && (args[0].Equals("/u", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("/uninstall", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("-u", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("-uninstall", StringComparison.InvariantCultureIgnoreCase)))
                {
                    UnInstallService(args);
                    return;
                }

                RunInConsoleMode();
            }
            else
            {
                RunAsService();
            }
        }

        private static void RunAsService()
        {
            int startUpIdleTime;
            int.TryParse(ConfigurationManager.AppSettings["StartupIdle"], out startUpIdleTime);
            _log.Debug("Running in service mode.");
            Thread.Sleep(startUpIdleTime);
            ServiceBase.Run(new[] { new MainService() });
        }

        private static void RunInConsoleMode()
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

        private static void InstallService(string[] args)
        {
            string serviceName = GetServiceName(args);

            List<string> parameters = new List<string>{Assembly.GetExecutingAssembly().Location, string.Format("/LogFile={0}", Assembly.GetExecutingAssembly().Location)};

            if (!serviceName.IsNullOrTimmedEmpty())
                parameters.Add(string.Format("--ServiceName={0}", serviceName));

            try
            {
                ManagedInstallerClass.InstallHelper(parameters.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured during install:");
                Console.WriteLine(ex.ToString());
            }
        }

        private static void UnInstallService(string[] args)
        {
            string serviceName = GetServiceName(args);

            List<string> parameters = new List<string> {Assembly.GetExecutingAssembly().Location, "/u", string.Format("/LogFile={0}", Assembly.GetExecutingAssembly().Location)};

            if (!serviceName.IsNullOrTimmedEmpty())
                parameters.Add(string.Format("--ServiceName={0}", serviceName));

            try
            {
                ManagedInstallerClass.InstallHelper(parameters.ToArray());            
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured during uninstall:");
                Console.WriteLine(ex.ToString());
            }
        }

        private static string GetServiceName(string[] args)
        {
            if (args.Length > 1 && args[1].StartsWith("/sn=", StringComparison.InvariantCultureIgnoreCase))
            {
                string serviceName = args[1].Substring(4).Trim().Trim('"');
                return serviceName.IsNullOrTimmedEmpty() ? null : serviceName;
            }

            if (args.Length > 1 && args[1].StartsWith("/servicename=", StringComparison.InvariantCultureIgnoreCase))
            {
                string serviceName = args[1].Substring(13).Trim().Trim('"');
                return serviceName.IsNullOrTimmedEmpty() ? null : serviceName;
            }

            return null;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.ErrorFormat("Unhandled error: {0}", e.ExceptionObject);
        }
    }
}
