using System;
using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.PluginSystem
{
    public abstract partial class TMSPSPluginBase
    {
        #region Properties

        internal static ChallengeInfo CurrentChallengeInfo { get; set; }
        private static ServerOptions CurrentServerOptions { get; set; }
        private static GameMode? CurrentGameMode { get; set; }
        protected int PlayersCount { get { return Context.PlayerSettings.Count; } }

        #endregion

        #region Methods

        protected static string GetEmptyManiaLinkPage(string manaiaLinkID)
        {
            return string.Format(@"<manialink id=""{0}""></manialink>", manaiaLinkID);
        }

        protected GenericResponse<bool> SendEmptyManiaLinkPage(string manaiaLinkID)
        {
            return Context.RPCClient.Methods.SendDisplayManialinkPage(GetEmptyManiaLinkPage(manaiaLinkID), 0, false);
        }

        protected GenericResponse<bool> SendEmptyManiaLinkPageToLogin(string login, string manaiaLinkID)
        {
            return Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetEmptyManiaLinkPage(manaiaLinkID), 0, false);
        }

        public ChallengeInfo GetCurrentChallengeInfoCached()
        {
            return CurrentChallengeInfo ?? GetCurrentChallengeInfo();
        }

        public ChallengeInfo GetCurrentChallengeInfo()
        {
            GenericResponse<ChallengeInfo> currentChallengeInfoResponse = Context.RPCClient.Methods.GetCurrentChallengeInfo();

            if (currentChallengeInfoResponse.Erroneous)
            {
                Logger.Error("Error getting current ChallengeInfo: " + currentChallengeInfoResponse.Fault.FaultMessage);
                Logger.ErrorToUI("An error occured during current challenge info retrieval!");
                return null;
            }

            // cache the ChallengeInfo
            CurrentChallengeInfo = currentChallengeInfoResponse.Value;

            return currentChallengeInfoResponse.Value;
        }

        protected List<ChallengeListSingleInfo> GetChallengeList()
        {
            GenericListResponse<ChallengeListSingleInfo> challengeResponse = Context.RPCClient.Methods.GetChallengeList(10000, 0);
            
            if (challengeResponse.Erroneous)
            {
                Logger.Error("Error getting ChallengeList: " + challengeResponse.Fault.FaultMessage);
                Logger.ErrorToUI("An error occured during challenge list retrieval!");
                return null;
            }

            return challengeResponse.Value;
        }

        protected static string GetNickname(string login)
        {
            return NicknameResolverFactory.Instance.Get(login);
        }


        protected PlayerInfo GetPlayerInfo(string login)
        {
            GenericResponse<PlayerInfo> playerInfoResponse = Context.RPCClient.Methods.GetPlayerInfo(login);

            if (playerInfoResponse.Erroneous)
            {
                Logger.Warn(string.Format("Error getting Playerinfo for player with login {0}: {1}", login, playerInfoResponse.Fault.FaultMessage));
                Logger.WarnToUI(string.Format("Error getting Playerinfo for player with login {0}", login));
                return null;
            }

            if (!login.Equals(Context.ServerInfo.ServerLogin, StringComparison.OrdinalIgnoreCase))
                GetPlayerSettings(login, true).UpdateFromPlayerInfo(playerInfoResponse.Value);

            return playerInfoResponse.Value;
        }

        protected List<PlayerInfo> GetPlayerList()
        {
            return GetPlayerList(this);
        }

        protected static List<PlayerInfo> GetPlayerList(TMSPSPluginBase plugin)
        {
            GenericListResponse<PlayerInfo> playersResponse = plugin.Context.RPCClient.Methods.GetPlayerList();

            if (playersResponse.Erroneous)
            {
                plugin.Logger.Error("Error getting PlayerList: " + playersResponse.Fault.FaultMessage);
                plugin.Logger.ErrorToUI("An error occured during player list retrieval!");
                return null;
            }

            foreach (PlayerInfo playerInfo in  playersResponse.Value)
            {
                plugin.GetPlayerSettings(playerInfo.Login, true).UpdateFromPlayerInfo(playerInfo);
            }

            return playersResponse.Value;
        }

        protected ServerOptions GetServerOptionsCached()
        {
            return GetServerOptionsCached(this);
        }

        protected ServerOptions GetServerOptions()
        {
            return GetServerOptions(this);
        }

        protected static ServerOptions GetServerOptionsCached(TMSPSPluginBase plugin)
        {
            return CurrentServerOptions ?? GetServerOptions(plugin);
        }

        protected static ServerOptions GetServerOptions(TMSPSPluginBase plugin)
        {
            GenericResponse<ServerOptions> serverOptionsResponse = plugin.Context.RPCClient.Methods.GetServerOptions();

            if (serverOptionsResponse.Erroneous)
            {
                plugin.Logger.Error("Error getting server options: " + serverOptionsResponse.Fault.FaultMessage);
                plugin.Logger.ErrorToUI("An error occured during server options retrieval!");
                return null;
            }

            CurrentServerOptions = serverOptionsResponse.Value;

            return serverOptionsResponse.Value;
        }

        protected GameMode? GetCurrentGameModeCached()
        {
            return GetCurrentGameModeCached(this);
        }

        protected static GameMode? GetCurrentGameModeCached(TMSPSPluginBase plugin)
        {
            return CurrentGameMode ?? GetCurrentGameMode(plugin);
        }

        protected GameMode? GetCurrentGameMode()
        {
            return GetCurrentGameMode(this);
        }

        protected static GameMode? GetCurrentGameMode(TMSPSPluginBase plugin)
        {
            GenericResponse<int> currentGameModeResponse = plugin.Context.RPCClient.Methods.GetGameMode();

            if (currentGameModeResponse.Erroneous)
            {
                plugin.Logger.Error("Error getting current game mode: " + currentGameModeResponse.Fault.FaultMessage);
                plugin.Logger.ErrorToUI("An error occured during current game mode retrieval!");
                return null;
            }

            CurrentGameMode = (GameMode?) currentGameModeResponse.Value;

            return CurrentGameMode;
        }

        protected DetailedPlayerInfo GetDetailedPlayerInfo(string login)
        {
            GenericResponse<DetailedPlayerInfo> playerInfoResponse = Context.RPCClient.Methods.GetDetailedPlayerInfo(login);

            if (playerInfoResponse.Erroneous)
            {
                Logger.Error(string.Format("Error getting detailed Playerinfo for player with login {0}: {1}", login, playerInfoResponse.Fault.FaultMessage));
                Logger.ErrorToUI(string.Format("Error getting detailed Playerinfo for player with login {0}", login));
                return null;
            }

            if (!login.Equals(Context.ServerInfo.ServerLogin, StringComparison.OrdinalIgnoreCase))
                GetPlayerSettings(login, true).UpdateFromPlayerInfo(playerInfoResponse.Value);

            return playerInfoResponse.Value;
        }

        protected List<PlayerRank> GetCurrentRanking()
        {
            GenericListResponse<PlayerRank> rankingResponse = Context.RPCClient.Methods.GetCurrentRanking();

            if (rankingResponse.Erroneous)
            {
                Logger.Error(string.Format("Error getting current ranking. : {0}", rankingResponse.Fault.FaultMessage));
                Logger.ErrorToUI("Error getting current ranking.");
                return null;
            }

            return rankingResponse.Value;
        }

        #endregion
    }
}