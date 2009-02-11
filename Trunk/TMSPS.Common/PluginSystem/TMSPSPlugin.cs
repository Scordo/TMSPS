using System.IO;

namespace TMSPS.Core.PluginSystem
{
	public abstract class TMSPSPlugin : TMSPSPluginBase, ITMSPSPlugin
    {
    	#region Properties

        protected string PluginDirectory { get { return Path.Combine(PluginsDirectory, ShortName); } }
        protected string PluginSettingsFilePath { get { return Path.Combine(PluginDirectory, "Settings.xml"); } }

    	#endregion
    }
}