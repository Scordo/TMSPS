namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class Ranking
	{
		#region Properties

        public string Nickname { get; set; }
		public string Login { get; set; }
        public uint CurrentRank { get; set; }
		public int PlayerID { get; set; }
        public double AverageRank { get; set; }
        public double RecordsCount { get; set; }
        public double ChallengesCount { get; set; }
        public double Score { get; set; }

		#endregion
	}

    public class RankingStats
    {
        #region Properties

        public string Nickname { get; set; }
        public int PlayerID { get; set; }
        public uint Amount { get; set; }

        #endregion

        #region Constructors

        public RankingStats()
            : this(null, 0, 0)
        {

        }

        public RankingStats(string nickname, int playerID, uint amount)
        {
            Nickname = nickname;
            PlayerID = playerID;
            Amount = amount;
        }

        #endregion
    }
}