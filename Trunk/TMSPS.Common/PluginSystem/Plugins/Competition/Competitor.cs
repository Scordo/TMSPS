namespace TMSPS.Core.PluginSystem.Plugins.Competition
{
    public class Competitor
    {
        public string Login { get; private set; }
        public int Score { get; set; }

        public Competitor(string login)
        {
            Login = login;
        }
    }
}