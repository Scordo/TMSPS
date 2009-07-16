using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class PlayerRank : IDump
	{
		[RPCParam("Login")]
		public string Login { get; set; }

		[RPCParam("NickName")]
		public string NickName { get; set; }

		[RPCParam("PlayerId")]
		public int PlayerId { get; set; }

		[RPCParam("Rank")]
		public int Rank { get; set; }

		[RPCParam("BestTime")]
		public int BestTime { get; set; }

		[RPCParam("Score")]
		public int Score { get; set; }

		[RPCParam("NbrLapsFinished")]
		public int NbrLapsFinished { get; set; }

		[RPCParam("LadderScore")]
		public double LadderScore { get; set; }

		[RPCParam("BestCheckpoints")]
		public List<int> BestCheckpoints { get; set; }
	}
}
