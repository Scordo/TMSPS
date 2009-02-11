namespace TMSPS.Core.PluginSystem
{
	public abstract class TMSPSPluginPlugin<T> : TMSPSPluginBase
	{
		#region Properties

		protected T HostPlugin { get; private set; }

		#endregion

		#region ILocalRecordsPluginPlugin Members

		public void ProvideHostPlugin(T hostPlugin)
		{
			HostPlugin = hostPlugin;
		}

		#endregion
	}
}
