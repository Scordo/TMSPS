namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public abstract class LocalRecordsPluginPlugin : TMSPSPluginPlugin<LocalRecordsPlugin>, ILocalRecordsPluginPlugin
	{
        #region Constructor

        protected LocalRecordsPluginPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion
	}
}