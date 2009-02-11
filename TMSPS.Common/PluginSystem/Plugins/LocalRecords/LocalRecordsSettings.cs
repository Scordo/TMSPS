using System;
using System.Xml.Linq;
using System.Configuration;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class LocalRecordsSettings
	{
		#region Properties

		public string ProviderAssemblyLocation { get; private set; }
		public string ProviderClass { get; private set; }
		public string ProviderParameter { get; private set; }
		public bool ShowMessages { get; private set; }

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

			return result;
		}

		#endregion
	}
}