using System.IO;

namespace TMSPS.Core.PluginSystem
{
	public abstract class TMSPSPluginPlugin<T> : TMSPSPluginBase where T : TMSPSPlugin
	{
		#region Properties

		protected T HostPlugin { get; private set; }
        public string HostPluginDirectory { get { return HostPlugin.PluginDirectory; } }
        public override string PluginDirectory { get { return Path.Combine(HostPluginDirectory, ShortName); } }

		#endregion

		#region ILocalRecordsPluginPlugin Members

		public void ProvideHostPlugin(T hostPlugin)
		{
			HostPlugin = hostPlugin;
		}

		#endregion
	}
}
