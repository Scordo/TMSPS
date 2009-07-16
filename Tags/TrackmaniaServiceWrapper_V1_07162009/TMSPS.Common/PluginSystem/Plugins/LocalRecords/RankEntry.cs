namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class RankEntry
    {
        #region Properties

        public ushort Rank { get; private set; }
        public string Login { get; private set; }
        public string Nickname { get; private set; }
        public uint TimeOrScore { get; private set; }

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
