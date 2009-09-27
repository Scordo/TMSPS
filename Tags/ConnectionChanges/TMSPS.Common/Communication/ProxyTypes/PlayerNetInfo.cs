using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class PlayerNetInfo : IDump
	{
		[RPCParam("Login")]
		public string Login { get; set; }

		[RPCParam("IPAddress")]
		public string IPAddress { get; set; }

		[RPCParam("StateUpdateLatency")]
		public int StateUpdateLatency { get; set; }

		[RPCParam("StateUpdatePeriod")]
		public int StateUpdatePeriod { get; set; }

		[RPCParam("LatestNetworkActivity")]
		public int LatestNetworkActivity { get; set; }

		[RPCParam("PacketLossRate")]
		public double PacketLossRate { get; set; }
	}
}
