using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class Mod
	{
		[RPCParam("EnvName")]
		public string EnvName { get; set; }

		[RPCParam("Url")]
		public string Url { get; set; }
	}
}