using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class ServerStatus : IDump
	{
		[RPCParam("Code")]
		public int Code { get; set; }

		[RPCParam("Name")]
		public string Name { get; set; }
	}
}