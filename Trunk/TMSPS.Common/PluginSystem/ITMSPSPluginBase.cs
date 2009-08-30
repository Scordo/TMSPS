using System;
using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.Logging;

namespace TMSPS.Core.PluginSystem
{
	public interface ITMSPSPluginBase
	{
        ushort ID { get; }
		Version Version { get; }
		string Author { get; }
		string Name { get; }
		string Description { get; }
		string ShortName { get; }
        IEnumerable<CommandHelp> CommandHelpList { get; }

		void InitPlugin(PluginHostContext context, IUILogger logger);
		void DisposePlugin(bool connectionLost);
	}
}
