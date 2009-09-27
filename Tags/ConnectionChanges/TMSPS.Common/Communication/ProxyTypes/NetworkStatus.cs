using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class NetworkStatus : IDump
	{
		[RPCParam("Uptime")]
		public int Uptime { get; set; }

		[RPCParam("NbrConnection")]
		public int NbrConnection { get; set; }

		[RPCParam("MeanConnectionTime")]
		public int MeanConnectionTime { get; set; }

		[RPCParam("MeanNbrPlayer")]
		public int MeanNbrPlayer { get; set; }

		[RPCParam("RecvNetRate")]
		public int RecvNetRate { get; set; }

		[RPCParam("SendNetRate")]
		public int SendNetRate { get; set; }

		[RPCParam("TotalReceivingSize")]
		public int TotalReceivingSize { get; set; }

		[RPCParam("TotalSendingSize")]
		public int TotalSendingSize { get; set; }

		[RPCParam("PlayerNetInfos")]
		public List<PlayerNetInfo> PlayerNetInfos { get; set; }
	}
}
