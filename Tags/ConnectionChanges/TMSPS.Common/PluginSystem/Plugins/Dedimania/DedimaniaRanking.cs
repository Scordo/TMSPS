using System;
using System.Diagnostics;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania
{
    [DebuggerDisplay("Login: {Login}, Nickname: {Nickname}, Created: {Created}, TimeOrScore: {TimeOrScore}")]
    public class DedimaniaRanking
    {
        #region Properties

        public uint TimeOrScore { get; set; }
        public DateTime Created { get; set; }
        public string Login { get; set; }
        public string Nickname { get; set; }

        #endregion

        #region Constructors

        public DedimaniaRanking()
        {
            
        }

        public DedimaniaRanking(string login, string nickname, uint timeOrScore, DateTime created)
        {
            Login = login;
            Nickname = nickname;
            TimeOrScore = timeOrScore;
            Created = created;
        }

        #endregion

        #region Public Methods

        public static int Comparer(DedimaniaRanking x, DedimaniaRanking y)
        {
            if (x.TimeOrScore != y.TimeOrScore)
                return Convert.ToInt32((int)x.TimeOrScore - (int)y.TimeOrScore);

            return x.Created.Ticks < y.Created.Ticks ? -1 : +1;
        }

        #endregion
    }
}