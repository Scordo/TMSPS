using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Logging;
using TMSPS.Core.PluginSystem.Configuration;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania
{
    public class DedimaniaSettings : SettingsBase
    {
        #region Constants

        public const string AUTH_URL = "http://dedimania.net/RPC4/server.php";
        public const string REPORT_URL = "http://dedimania.net:8015/Dedimania";
        private const uint MAX_RECORDS_TO_REPORT = 30; // values above this value wont be accepted

        #endregion

        #region Properties

        public string AuthUrl { get; private set; }
        public string ReportUrl { get; private set; }
        public uint MaxRecordsToReport { get; private set; }

        #endregion

        #region Public Methods

        public static DedimaniaSettings ReadFromFile(string xmlConfigurationFile)
        {
            DedimaniaSettings result = new DedimaniaSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.AuthUrl = ReadConfigString(configDocument.Root, "AuthUrl", AUTH_URL, xmlConfigurationFile);
            result.ReportUrl = ReadConfigString(configDocument.Root, "ReportUrl", REPORT_URL, xmlConfigurationFile);
            result.MaxRecordsToReport = Math.Min(ReadConfigUInt(configDocument.Root, "MaxRecordsToReport", MAX_RECORDS_TO_REPORT, xmlConfigurationFile), MAX_RECORDS_TO_REPORT);
            result.Plugins = PluginConfigEntryCollection.ReadFromXElement(configDocument.Root.Element("Plugins"));

            return result;
        }

        public List<IDedimaniaPluginPlugin> GetPlugins(IUILogger logger)
        {
            List<IDedimaniaPluginPlugin> result = new List<IDedimaniaPluginPlugin>();

            foreach (PluginConfigEntry pluginConfigEntry in Plugins.GetEnabledPlugins())
            {
                if (logger != null)
                    logger.Debug(string.Format("Instantiating IDedimaniaPluginPlugin {0}", pluginConfigEntry.PluginClass));

                result.Add(Instancer.GetInstanceOfInterface<IDedimaniaPluginPlugin>(pluginConfigEntry.AssemblyName, pluginConfigEntry.PluginClass));
            }

            return result;
        }

        #endregion
    }
}