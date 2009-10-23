using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class SystemInfo : IDump
	{
		[RPCParam("PublishedIp")]
		public string PublishedIp { get; set; }

		[RPCParam("Port")]
		public int Port { get; set; }

		[RPCParam("P2PPort")]
		public int P2PPort { get; set; }

		[RPCParam("ServerLogin")]
		public string ServerLogin { get; set; }

		[RPCParam("ServerPlayerId")]
		public int ServerPlayerId { get; set; }
	}
}
