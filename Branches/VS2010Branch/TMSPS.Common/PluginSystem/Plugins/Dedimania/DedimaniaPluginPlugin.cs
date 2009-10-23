namespace TMSPS.Core.PluginSystem.Plugins.Dedimania
{
    public abstract class DedimaniaPluginPlugin : TMSPSPluginPlugin<DedimaniaPlugin>, IDedimaniaPluginPlugin
    {
        #region Constructor

        protected DedimaniaPluginPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion
    }
}