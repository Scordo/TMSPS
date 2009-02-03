using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class ForcedSkin : IDump
	{
		[RPCParam("Orig")]
		public string Orig { get; set; }

		[RPCParam("Name")]
		public string Name { get; set; }

		[RPCParam("Checksum")]
		public string Checksum { get; set; }

		[RPCParam("Url")]
		public string Url { get; set; }
	}
}
