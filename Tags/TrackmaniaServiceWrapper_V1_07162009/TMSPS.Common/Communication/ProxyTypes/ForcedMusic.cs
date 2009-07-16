using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class ForcedMusic : IDump
	{
		[RPCParam("Override")]
		public bool Override { get; set; }

		[RPCParam("Url")]
		public string Url { get; set; }

		[RPCParam("File")]
		public string File { get; set; }
	}
}
