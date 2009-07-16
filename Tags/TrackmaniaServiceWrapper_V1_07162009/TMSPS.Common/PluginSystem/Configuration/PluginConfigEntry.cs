using System;
using System.Configuration;
using System.Globalization;
using System.Xml.Linq;

namespace TMSPS.Core.PluginSystem.Configuration
{
	public class PluginConfigEntry
	{
		#region Properties

		public bool Enabled { get; private set; }
		public string AssemblyName { get; private set; }
		public string PluginClass { get; private set; }

		#endregion

		#region Constructor

		private PluginConfigEntry()
		{
			Enabled = true;
		}

		#endregion

		#region Public Methods

		public static PluginConfigEntry ReadFromXmlString(string xmlElementString)
		{
			return ReadFromXElement(XElement.Parse(xmlElementString));
		}

		public static PluginConfigEntry ReadFromXElement(XElement pluginElement)
		{
			if (pluginElement.Name != "Plugin")
				throw new ConfigurationErrorsException("Elementname is not 'Plugin', it's: " + pluginElement.Name);

			XAttribute assemblyNameAttribute = pluginElement.Attribute("assemblyName");

			if (assemblyNameAttribute == null)
				throw new ConfigurationErrorsException("Could not find 'assemblyName' attribute!");

			if (assemblyNameAttribute.Value.IsNullOrTimmedEmpty())
				throw new ConfigurationErrorsException("'assemblyName' attribute is emtpy!");

			XAttribute pluginClassAttribute = pluginElement.Attribute("pluginClass");

			if (pluginClassAttribute == null)
				throw new ConfigurationErrorsException("Could not find 'pluginClass' attribute!");

			if (pluginClassAttribute.Value.IsNullOrTimmedEmpty())
				throw new ConfigurationErrorsException("'pluginClass' attribute is emtpy!");

			XAttribute enabledAttribute = pluginElement.Attribute("enabled");

			return new PluginConfigEntry
	       	{
				Enabled = (enabledAttribute == null) || (string.Compare(enabledAttribute.Value, "true", true, CultureInfo.InvariantCulture) == 0),
				PluginClass = pluginClassAttribute.Value.Trim(),
				AssemblyName = assemblyNameAttribute.Value.Trim()
	       	};
		}

		#endregion
	}
}
