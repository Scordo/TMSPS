using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class StartServerParameters
    {
        #region Properties

        [RPCParam("Login")]
        public string Login { get; set; }
        [RPCParam("Password")]
        public string Password { get; set; }

        #endregion

        #region Constructor

        public StartServerParameters()
        {
            
        }

        public StartServerParameters(string login, string password)
        {
            Login = login;
            Password = password;
        }

        #endregion
    }
}