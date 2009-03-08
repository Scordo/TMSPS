using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using log4net;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Configuration;
using TMSPS.Core.PluginSystem.Plugins;

namespace TMSPS.Core.PluginSystem
{
    public class ConfigSettingsConfigurationSection : ConfigurationSection
    {
        #region Non Public Members

        private static readonly ILog _log = LogManager.GetLogger(typeof(ConfigSettingsConfigurationSection));

        #endregion

        #region Config Properties

        [ConfigurationProperty("isDev", IsRequired = false, DefaultValue = false)]
        public bool IsDev
        {
            get { return (bool)base["isDev"]; }
        }

        [ConfigurationProperty("serverAddress", IsRequired = true)]
        public string ServerAddress
        {
            get { return (string)base["serverAddress"]; }
        }

        [ConfigurationProperty("serverNation", IsRequired = true)]
        public string ServerNation
        {
            get { return (string)base["serverNation"]; }
        }

        [ConfigurationProperty("serverLogin", IsRequired = true)]
        public string ServerLogin
        {
            get { return (string)base["serverLogin"]; }
        }

        [ConfigurationProperty("serverLoginPassword", IsRequired = true)]
        public string ServerLoginPassword
        {
            get { return (string)base["serverLoginPassword"]; }
        }

        [ConfigurationProperty("serverXMLRPCPort", IsRequired = true)]
        public ushort ServerXMLRPCPort
        {
            get { return (ushort)base["serverXMLRPCPort"]; }
        }

        [ConfigurationProperty("superAdminPassword", IsRequired = true)]
        public string SuperAdminPassword
        {
            get { return (string)base["superAdminPassword"]; }
        }

        #endregion

        #region Public Methods

        public List<ITMSPSPlugin> GetPlugins()
        {
        	string mainDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string pluginsSettingsFile = Path.Combine(mainDirectory, @"Plugins\Settings.xml");
			
            List<ITMSPSPlugin> result = new List<ITMSPSPlugin>();
            result.Add(new TMSPSCorePlugin());

			foreach (PluginConfigEntry pluginConfigEntry in PluginUtil.GetEnabledPlugins(pluginsSettingsFile))
            {
                _log.Debug(string.Format("Instantiating ITMSPSPlugin {0}", pluginConfigEntry.PluginClass));
                result.Add(Instancer.GetInstanceOfInterface<ITMSPSPlugin>(pluginConfigEntry.AssemblyName, pluginConfigEntry.PluginClass));
            }

            return result;
        }

        public static ConfigSettingsConfigurationSection GetFromConfig(string sectionName)
        {
            try
            {
                ConfigSettingsConfigurationSection config = (ConfigSettingsConfigurationSection)ConfigurationManager.GetSection(sectionName);

                if (config == null)
                {
                    _log.Error("Could not find ConfigSettingsSection.");
                    throw new ConfigurationErrorsException("Could not find ConfigSettingsSection.");
                }

                return config;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error while reading ConfigSettings from config file: {0}", sectionName, ex);
                throw;
            }
        }
        #endregion

        #region Non Public Methods

        protected override void PostDeserialize()
        {
            if (ServerAddress.IsNullOrTimmedEmpty())
                throw new InvalidOperationException("Please add a non empty server address for attribute 'serverAddress'.");

            if (ServerNation.IsNullOrTimmedEmpty())
                throw new InvalidOperationException("Please add a non empty server nation for attribute 'serverNation'.");

            if (ServerLogin.IsNullOrTimmedEmpty())
                throw new InvalidOperationException("Please add a non empty server login for attribute 'serverLogin'.");

            if (ServerLoginPassword.IsNullOrTimmedEmpty())
                throw new InvalidOperationException("Please add a non empty server login password for attribute 'serverLoginPassword'.");

            if (SuperAdminPassword.IsNullOrTimmedEmpty())
                throw new InvalidOperationException("Please add non empty super admin password for attribute 'superAdminPassword'.");
        }

        #endregion
    }
}