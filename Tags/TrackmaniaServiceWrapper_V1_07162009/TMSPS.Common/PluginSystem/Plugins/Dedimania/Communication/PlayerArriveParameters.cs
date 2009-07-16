namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class PlayerArriveParameters
    {
        public string Game { get; set; }
        public string Login { get; set; }
        public string Nickname { get; set; }
        public string Nation { get; set; }
        public string TeamName { get; set; }
        public int LadderRanking { get; set; }
        public bool Spectating { get; set; }
        public bool IsOff { get; set; }

        public PlayerArriveParameters()
        {
            
        }

        public PlayerArriveParameters(string game, string login, string nickname, string nation, string teamName, int ladderRanking, bool spectating, bool isOff)
        {
            Game = game;
            Login = login;
            Nickname = nickname;
            Nation = nation;
            TeamName = teamName;
            LadderRanking = ladderRanking;
            Spectating = spectating;
            IsOff = isOff;
        }
    }
}
