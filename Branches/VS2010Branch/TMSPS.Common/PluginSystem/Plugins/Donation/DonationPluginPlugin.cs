namespace TMSPS.Core.PluginSystem.Plugins.Donation
{
    public abstract class DonationPluginPlugin : TMSPSPluginPlugin<DonationPlugin>, IDonationPluginPlugin
    {
        #region Constructor

        protected DonationPluginPlugin(string pluginDirectory) : base(pluginDirectory)
        {

        }

        #endregion
    }
}