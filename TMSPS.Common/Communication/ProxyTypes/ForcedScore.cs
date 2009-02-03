using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class ForcedScore : IDump
	{
		[RPCParam("PlayerId")]
		public int PlayerId { get; set; }

		[RPCParam("Score")]
		public double Score { get; set; }
	}
}
