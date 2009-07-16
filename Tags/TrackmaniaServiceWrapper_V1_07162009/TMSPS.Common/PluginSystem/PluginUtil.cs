using System;
using System.Xml.Linq;
using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.Core.PluginSystem
{
	public class PluginUtil
	{
		public static PluginConfigEntryCollection GetEnabledPlugins(string settingFilePath)
		{
			XDocument settingsDocument = XDocument.Load(settingFilePath);

			if (settingsDocument.Root == null)
				throw new InvalidOperationException("Root document node is not present!");

			return PluginConfigEntryCollection.ReadFromXElement(settingsDocument.Root.Element("Plugins")).GetEnabledPlugins();
		}
	}
}
