using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class CallVote : IDump
	{
		[RPCParam("CallerLogin")]
		public string CallerLogin { get; set; }

		[RPCParam("CmdName")]
		public string CmdName { get; set; }

		[RPCParam("CmdParam")]
		public string CmdParam { get; set; }
	}
}