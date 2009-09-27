using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class LanguageDependentText : IDump
	{
		[RPCParam("Lang")]
		public string Lang { get; set; }

		[RPCParam("Text")]
		public string Text { get; set; }
	}
}
