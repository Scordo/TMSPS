using System;
using TMSPS.Core.Logging;

namespace TMSPS.Core.PluginSystem
{
    public interface ITMSPSPlugin
    {
        Version Version { get; }
        string Author { get; }
        string Name { get; }
        string Description { get; }
		string ShortNameForLogging { get; }

        void InitPlugin(PluginHostContext context, IUILogger logger);
        void DisposePlugin();
    }
}