namespace TMSPS.Core.PluginSystem
{
	public abstract class TMSPSPlugin : TMSPSPluginBase, ITMSPSPlugin
    {
    	#region Constructor

        protected TMSPSPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion
    }
}