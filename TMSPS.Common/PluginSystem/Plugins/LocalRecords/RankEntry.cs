namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class RankEntry
    {
        #region Properties

        public ushort Rank { get; set; }
        public string Login { get; set; }
        public string Nickname { get; set; }
        public uint TimeOrScore { get; set; }

        #endregion

        #region Constructor

        public RankEntry(ushort rank, string login, string nickname, uint timeOrScore)
        {
            Rank = rank;
            Login = login;
            Nickname = nickname;
            TimeOrScore = timeOrScore;
        }

        #endregion
    }
}
