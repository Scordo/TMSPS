
namespace TMSPS.Core.PluginSystem
{
	public interface ITMSPSPluginPlugin<T> : ITMSPSPluginBase
	{
		void ProvideHostPlugin(T hostPlugin);
	}
}
