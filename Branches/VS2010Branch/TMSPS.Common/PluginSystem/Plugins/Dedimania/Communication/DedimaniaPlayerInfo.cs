namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaPlayerInfo
    {
        public string Login { get; set; }
        public string Nation { get; set; }
        public string TeamName { get; set; }
        public int TeamId { get; set; }
        public bool IsSpec { get; set; }
        public int Ranking { get; set; }
        public bool IsOff { get; set; }

        public DedimaniaPlayerInfo()
        {

        }

        public DedimaniaPlayerInfo(string login, string nation, string teamName, int teamId, bool isSpec, int ranking, bool isOff)
        {
            Login = login;
            Nation = nation;
            TeamName = teamName;
            TeamId = teamId;
            IsSpec = isSpec;
            Ranking = ranking;
            IsOff = isOff;
        }
    }
}