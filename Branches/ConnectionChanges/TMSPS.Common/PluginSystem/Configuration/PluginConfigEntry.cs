using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

namespace TMSPS.Core.PluginSystem.Configuration
{
    [DebuggerDisplay("{PluginClass} - {AssemblyName}")]
	public class PluginConfigEntry
	{
		#region Properties

		public bool Enabled { get; private set; }
		public string AssemblyName { get; private set; }
		public string PluginClass { get; private set; }
        public ushort Order { get; private set; }
        public string PluginDirectory { get; private set; }

		#endregion

		#region Constructor

		private PluginConfigEntry()
		{
			Enabled = true;
            Order = ushort.MaxValue;
		}

		#endregion

		#region Public Methods

		public static PluginConfigEntry ReadFromXmlString(string xmlElementString, string pluginDirectory)
		{
            return ReadFromXElement(XElement.Parse(xmlElementString), pluginDirectory);
		}

        public static PluginConfigEntry ReadFromXElement(XElement pluginElement, string pluginDirectory)
		{
            if (pluginElement.Name != "Settings")
                throw new ConfigurationErrorsException("Elementname is not 'Settings', it's: " + pluginElement.Name);

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

            XAttribute orderAttribute = pluginElement.Attribute("order");

            ushort order = ushort.MaxValue;

            if (orderAttribute != null && !ushort.TryParse(orderAttribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out order))
                order = ushort.MaxValue;


			XAttribute enabledAttribute = pluginElement.Attribute("enabled");

			return new PluginConfigEntry
	       	{
				Enabled = (enabledAttribute == null) || (string.Compare(enabledAttribute.Value, "true", true, CultureInfo.InvariantCulture) == 0),
				PluginClass = pluginClassAttribute.Value.Trim(),
				AssemblyName = assemblyNameAttribute.Value.Trim(),
                Order = order,
                PluginDirectory = pluginDirectory
	       	};
		}

		#endregion
	}
}
