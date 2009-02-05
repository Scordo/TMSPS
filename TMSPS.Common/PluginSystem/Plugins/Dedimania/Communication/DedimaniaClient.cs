using System;
using System.Collections.Generic;
using CookComputing.XmlRpc;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaClient
    {
        private IDedimaniaProxy Proxy { get; set; }
        private AuthenticateParameters AuthParameters { get; set; }

        internal string Url
        {
            get { return Proxy.Url; }
            set { Proxy.Url = value;}
        }

        public DedimaniaClient(string url, AuthenticateParameters authParameters)
        {
            AuthParameters = authParameters;

            Proxy = XmlRpcProxyGen.Create<IDedimaniaProxy>();
            Proxy.UserAgent = "TMSPS";
            Proxy.Url = url;
            Proxy.EnableCompression = true;
            Proxy.KeepAlive = true;
        }

        public DedimaniaVersionInfoReply GetVersion()
        {
            return Proxy.GetVersion();
        }

        public bool Authenticate()
        {
            return Proxy.Authenticate(AuthParameters);
        }


        public DedimaniaWarningsAndTTRReply GetWarningsAndTTR()
        {
            return Proxy.GetWarningsAndTTR();
        }

        public bool ValidateAccount()
        {
            object[] multiCallRawResult = Proxy.MultiCall(new[]
            {
               new RPCMethodInfo("dedimania.Authenticate", AuthParameters), 
               new RPCMethodInfo("dedimania.ValidateAccount"),
               new RPCMethodInfo("dedimania.WarningsAndTTR")
            });

            MultiCallResult multiCallResult = MultiCallResult.Parse(multiCallRawResult);
            if (multiCallResult == null)
                return false;

            FaultInfo faultInfo = FaultInfo.Parse(multiCallRawResult[1]);

            if (faultInfo != null)
                return false;

            return true;
        }

        public DedimaniaPlayerLeaveReply PlayerLeave(string game, string login)
        {
            object[] multiCallRawResult = Proxy.MultiCall(new[]
            {
               new RPCMethodInfo("dedimania.Authenticate", AuthParameters), 
               new RPCMethodInfo("dedimania.PlayerLeave", game, login),
               new RPCMethodInfo("dedimania.WarningsAndTTR")
            });
    
            MultiCallResult multiCallResult = MultiCallResult.Parse(multiCallRawResult);
            if (multiCallResult == null)
                return null;

            FaultInfo faultInfo = FaultInfo.Parse(multiCallRawResult[1]);

            if (faultInfo != null)
                return null;

            return DedimaniaPlayerLeaveReply.Parse(multiCallRawResult[1]);
        }

        public bool UpdateServerPlayers(string game, int mode, DedimaniaServerInfo serverInfo, DedimaniaPlayerInfo[] players)
        {
            object[] multiCallRawResult = Proxy.MultiCall(new[]
            {
               new RPCMethodInfo("dedimania.Authenticate", AuthParameters), 
               new RPCMethodInfo("dedimania.UpdateServerPlayers", game, mode, serverInfo, players),
               new RPCMethodInfo("dedimania.WarningsAndTTR")
            });

            MultiCallResult multiCallResult = MultiCallResult.Parse(multiCallRawResult);
            if (multiCallResult == null)
                return false;

            FaultInfo faultInfo = FaultInfo.Parse(multiCallRawResult[1]);

            if (faultInfo != null)
                return false;

            return ParseBool(multiCallRawResult[1]);
        }

        public DedimaniaCurrentChallengeReply CurrentChallenge(string uid, string name, string environment, string author, string game, int mode, DedimaniaServerInfo serverInfo, int maxGetTimes, DedimaniaPlayerInfo[] players)
        {
            object[] multiCallRawResult = Proxy.MultiCall(new[]
            {
               new RPCMethodInfo("dedimania.Authenticate", AuthParameters), 
               new RPCMethodInfo("dedimania.CurrentChallenge", uid, name, environment, author, game, mode, serverInfo, maxGetTimes, players),
               new RPCMethodInfo("dedimania.WarningsAndTTR")
            });

            MultiCallResult multiCallResult = MultiCallResult.Parse(multiCallRawResult);
            if (multiCallResult == null)
                return null;

            FaultInfo faultInfo = FaultInfo.Parse(multiCallRawResult[1]);

            if (faultInfo != null)
                return null;

            return DedimaniaCurrentChallengeReply.Parse(multiCallRawResult[1]);
        }

        public DedimaniaChallengeRaceTimesReply ChallengeRaceTimes(string uid, string name, string environment, string author, string game, int mode, int numberOfChecks, int maxGetTimes, DedimaniaTime[] times)
        {
            object[] multiCallRawResult = Proxy.MultiCall(new[]
            {
               new RPCMethodInfo("dedimania.Authenticate", AuthParameters), 
               new RPCMethodInfo("dedimania.ChallengeRaceTimes", uid, name, environment, author, game, mode, numberOfChecks, maxGetTimes, times),
               new RPCMethodInfo("dedimania.WarningsAndTTR")
            });

            MultiCallResult multiCallResult = MultiCallResult.Parse(multiCallRawResult);
            if (multiCallResult == null)
                return null;

            FaultInfo faultInfo = FaultInfo.Parse(multiCallRawResult[1]);

            if (faultInfo != null)
                return null;

            return DedimaniaChallengeRaceTimesReply.Parse(multiCallRawResult[1]);
        }

        public DedimaniaPlayerArriveReply PlayerArrive(string game, string login, string nickname, string nation, string teamName, int ladderRanking, bool spectating, bool isOff)
        {
            return PlayersArrive(new PlayerArriveParameters(game, login, nickname, nation, teamName, ladderRanking, spectating, isOff))[0];
        }

        public List<DedimaniaPlayerArriveReply> PlayersArrive(params PlayerArriveParameters[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                throw new ArgumentException("Parameters are empty");

            RPCMethodInfo[] methodCalls = new RPCMethodInfo[parameters.Length + 2];
            methodCalls[0] = new RPCMethodInfo("dedimania.Authenticate", AuthParameters);
            methodCalls[methodCalls.Length - 1] = new RPCMethodInfo("dedimania.WarningsAndTTR");

            for (int i = 0; i < parameters.Length; i++)
            {
                PlayerArriveParameters arriveParameters = parameters[i];
                methodCalls[i + 1] = new RPCMethodInfo("dedimania.PlayerArrive", arriveParameters.Game, arriveParameters.Login, arriveParameters.Nickname, arriveParameters.Nation, arriveParameters.TeamName, arriveParameters.LadderRanking, arriveParameters.Spectating, arriveParameters.IsOff);
            }

            object[] multiCallRawResult = Proxy.MultiCall(methodCalls);

            MultiCallResult multiCallResult = MultiCallResult.Parse(multiCallRawResult);
            if (multiCallResult == null)
                return null;

            List<DedimaniaPlayerArriveReply> result = new List<DedimaniaPlayerArriveReply>();

            for (int i = 1; i < multiCallRawResult.Length-1; i++)
            {
                result.Add(DedimaniaPlayerArriveReply.Parse(multiCallRawResult[i]));
            }

            return result;
        }

        private static bool ParseBool(object multiCallResultElement)
        {
            return (multiCallResultElement != null && multiCallResultElement.GetType() == typeof(bool[]) && ((bool[])multiCallResultElement).Length == 1 && ((bool[])multiCallResultElement)[0]);
        }

        #region Embedded Classes

        public interface IDedimaniaProxy : IXmlRpcProxy
        {
            [XmlRpcMethod("system.multicall")]
            object[] MultiCall(RPCMethodInfo[] calls);

            [XmlRpcMethod("dedimania.GetVersion")]
            DedimaniaVersionInfoReply GetVersion();

            [XmlRpcMethod("dedimania.Authenticate")]
            bool Authenticate(AuthenticateParameters authInfo);

            [XmlRpcMethod("dedimania.WarningsAndTTR")]
            DedimaniaWarningsAndTTRReply GetWarningsAndTTR();
        }

        public class RPCMethodInfo
        {
            [XmlRpcMember("methodName")]
            public string MethodName { get; set; }

            [XmlRpcMember("params")]
            public object[] Parameter { get; set; }

            public RPCMethodInfo(string methodName, params object[] parameters)
            {
                MethodName = methodName;
                Parameter = parameters;
            }
        }

        #endregion
    }
}
