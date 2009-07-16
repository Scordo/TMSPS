using System.Xml.Linq;
using System.Collections.Generic;

namespace TMSPS.Core.PluginSystem.Configuration
{
	public class PluginConfigEntryCollection : List<PluginConfigEntry>
	{
		#region Constructor

		private PluginConfigEntryCollection()
		{
			
		}

		#endregion

		#region Public Methods

		public static PluginConfigEntryCollection ReadFromXmlString(string xmlElementString)
		{
			if (xmlElementString == null)
				return new PluginConfigEntryCollection();

			return ReadFromXElement(XElement.Parse(xmlElementString));
		}

		public static PluginConfigEntryCollection ReadFromXElement(XElement pluginsElement)
		{
			if (pluginsElement == null)
				return new PluginConfigEntryCollection();

			PluginConfigEntryCollection result = new PluginConfigEntryCollection();

			foreach (XElement pluginElement in pluginsElement.Elements("Plugin"))
			{
				result.Add(PluginConfigEntry.ReadFromXElement(pluginElement));
			}

			return result;
		}

		public PluginConfigEntryCollection GetEnabledPlugins()
		{
			PluginConfigEntryCollection result = new PluginConfigEntryCollection();
			result.AddRange(FindAll(plugin => plugin.Enabled));

			return result;
		}

		#endregion
	}
}