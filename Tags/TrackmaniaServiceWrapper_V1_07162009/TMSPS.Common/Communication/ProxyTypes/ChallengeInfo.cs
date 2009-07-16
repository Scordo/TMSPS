using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
	public class ChallengeInfo : ChallengeListSingleInfo
	{
		[RPCParam("Mood")]
		public string Mood { get; set; }

		[RPCParam("BronzeTime")]
		public int BronzeTime { get; set; }

		[RPCParam("SilverTime")]
		public int SilverTime { get; set; }

		[RPCParam("AuthorTime")]
		public int AuthorTime { get; set; }

		[RPCParam("LapRace")]
		public bool LapRace { get; set; }

        [RPCParam("NbLaps")]
        public int NumberOfLaps { get; set; }

        [RPCParam("NbCheckpoints")]
        public int NumberOfCheckpoints { get; set; }
	}
}