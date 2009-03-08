using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class ForcedMod : IDump
	{
		[RPCParam("Override")]
		public bool Override { get; set; }

		[RPCParam("Mods")]
		public List<Mod> Mods { get; set; }
	}
}