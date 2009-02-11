using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Configuration;
using TMSPS.Core.Common;
using TMSPS.Core.Logging;
using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class LocalRecordsSettings
	{
		#region Properties

		public string ProviderAssemblyLocation { get; private set; }
		public string ProviderClass { get; private set; }
		public string ProviderParameter { get; private set; }
		public bool ShowMessages { get; private set; }
		private PluginConfigEntryCollection Plugins { get; set; }

		#endregion

		#region Constructors

		private LocalRecordsSettings()
		{
			
		}

		#endregion

		#region Public Methods

		public static LocalRecordsSettings ReadFromFile(string xmlConfigurationFile)
		{
			LocalRecordsSettings result = new LocalRecordsSettings();
			XDocument configDocument = XDocument.Load(xmlConfigurationFile);

			if (configDocument.Root == null)
				throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

			XElement providerSettingsElement = configDocument.Root.Element("ProviderSettings");

			if (providerSettingsElement == null)
				throw new ConfigurationErrorsException("Could not find ProviderSettings node in file: " + xmlConfigurationFile);

			XElement assemblyElement = providerSettingsElement.Element("Assembly");

			if (assemblyElement == null)
				throw new ConfigurationErrorsException("Could not find Assembly node in file: " + xmlConfigurationFile);

			if (assemblyElement.Value.IsNullOrTimmedEmpty())
				throw new ConfigurationErrorsException("Assembly node is empty in file: " + xmlConfigurationFile);

			result.ProviderAssemblyLocation = assemblyElement.Value.Trim();

			XElement providerElement = providerSettingsElement.Element("Provider");

			if (providerElement == null)
				throw new ConfigurationErrorsException("Could not find Provider node in file: " + xmlConfigurationFile);

			if (providerElement.Value.IsNullOrTimmedEmpty())
				throw new ConfigurationErrorsException("Provider node is empty in file: " + xmlConfigurationFile);

			result.ProviderClass = providerElement.Value.Trim();

			XElement parameterElement = providerSettingsElement.Element("Parameter");

			if (parameterElement == null)
				throw new ConfigurationErrorsException("Could not find Parameter node in file: " + xmlConfigurationFile);

			result.ProviderParameter = parameterElement.Value.Trim();


			bool showMessages = true;
            XElement showMessagesElement = configDocument.Root.Element("ShowMessages");

			if (showMessagesElement != null)
				showMessages = string.Compare(showMessagesElement.Value.Trim(), "true", StringComparison.InvariantCultureIgnoreCase) == 0;

			result.ShowMessages = showMessages;
			result.Plugins = PluginConfigEntryCollection.ReadFromXElement(configDocument.Root.Element("Plugins"));

			return result;
		}

		public List<ILocalRecordsPluginPlugin> GetPlugins(IUILogger logger)
		{
			List<ILocalRecordsPluginPlugin> result = new List<ILocalRecordsPluginPlugin>();

			foreach (PluginConfigEntry pluginConfigEntry in Plugins.GetEnabledPlugins())
			{
				if (logger != null)
					logger.Debug(string.Format("Instantiating ILocalRecordsPluginPlugin {0}", pluginConfigEntry.PluginClass));

				result.Add(Instancer.GetInstanceOfInterface<ILocalRecordsPluginPlugin>(pluginConfigEntry.AssemblyName, pluginConfigEntry.PluginClass));
			}

			return result;
		}

		#endregion
	}
}