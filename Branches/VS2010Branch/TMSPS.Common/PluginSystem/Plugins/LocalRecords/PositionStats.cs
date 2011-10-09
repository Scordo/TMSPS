namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class PositionStats
    {
        #region Properties

        public int PlayerId { get; set; }
        public string Nickname { get; set; }
        public uint Amount { get; set; }

        #endregion

        #region Constructors

        public PositionStats()
            : this(0, null, 0)
        {

        }

        public PositionStats(int playerID, string nickname, uint amount)
        {
            PlayerId = playerID;
            Nickname = nickname;
            Amount = amount;
        }

        #endregion
    }
}