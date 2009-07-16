using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class CallVoteRatio : IDump
	{
		[RPCParam("Command")]
		public string Command { get; set; }

		[RPCParam("Ratio")]
		public double Ratio { get; set; }
	}
}
