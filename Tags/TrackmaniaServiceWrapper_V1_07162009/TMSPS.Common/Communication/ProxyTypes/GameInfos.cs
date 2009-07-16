using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class GameInfos : IDump
	{
		[RPCParam("CurrentGameInfos")]
		public GameInfo CurrentGameInfos { get; set; }

		[RPCParam("NextGameInfos")]
		public GameInfo NextGameInfos { get; set; }
	}
}
