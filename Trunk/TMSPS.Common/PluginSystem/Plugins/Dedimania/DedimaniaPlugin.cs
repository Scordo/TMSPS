using System;
using System.Collections.Generic;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;
using Timer=System.Threading.Timer;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania
{
    public class DedimaniaPlugin : TMSPSPlugin
    {
        #region Constants

        private const int UPDATE_SERVER_PLAYERS_INTERVAL_IN_MINUTES = 4;
        private const string TOOL_NAME = "TMSPS";

        #endregion

        #region Members

        private DedimaniaClient DedimaniaClient { get; set; }
        private Timer UpdateServerPlayersTimer { get; set; }

        #endregion

        #region Properties

        public override Version Version
        {
            get { return new Version("0.1"); }
        }

        public override string Author
        {
            get { return "Jens Hofmann"; }
        }

        public override string Name
        {
            get { return "DedimaniaPlugin"; }
        }

        public override string Description
        {
            get { return "Saves records in dedimania database."; }
        }

		public override string ShortName
		{
			get { return "Dedimania"; }
		}

        public DedimaniaSettings Settings { get; private set; }

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = DedimaniaSettings.ReadFromFile(PluginSettingsFilePath);

            AuthenticateParameters authParams = new AuthenticateParameters
            {
                Game = Context.ServerInfo.Version.GetShortName(),
                Login = Context.ServerInfo.ServerLogin,
                Packmask = Context.ServerInfo.ServerPackMask,
                Nation = Context.ServerInfo.ServerNation,
                Password = Context.ServerInfo.ServerLoginPassword,
                Tool = TOOL_NAME,
                Version = Version.ToString(2)
            };

            DedimaniaClient = new DedimaniaClient(Settings.AuthUrl, authParams);

            if (!DedimaniaClient.Authenticate())
            {
				Logger.ErrorToUI("Auth failed. Stopping Plugin!");
                return;
            }

            DedimaniaClient.Url = Settings.ReportUrl;
            Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;

            ResetUpdateServerPlayersTimer();
            ReportCurrentChallenge();
        }

        protected override void Dispose()
        {
            Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
        }

        private static void UpdateServerPlayers(object state)
        {
			DedimaniaPlugin plugin = (DedimaniaPlugin) state;
			RunCatchLog(()=>
			{
                ServerOptions serverOptions = GetServerOptions(plugin);
                if (serverOptions == null)
                    return;

                int? currentGameMode = GetCurrentGameMode(plugin);
                if (!currentGameMode.HasValue)
                    return;

                List<PlayerInfo> playerList = GetPlayerList(plugin);
                if (playerList == null)
                    return;

                List<PlayerInfo> currentPlayers = playerList;

				List<PlayerInfo> nonSpectators = currentPlayers.FindAll(player => !player.IsSpectator);
				List<DedimaniaPlayerInfo> playersToReport = new List<DedimaniaPlayerInfo>();

				foreach (PlayerInfo player in nonSpectators)
				{
					playersToReport.Add(new DedimaniaPlayerInfo(player.Login, string.Empty, string.Empty, player.TeamId, player.IsSpectator, player.LadderRanking, player.IsInOfficialMode));
				}

				int playersCount = playersToReport.Count;
				int spectatorsCount = currentPlayers.Count - playersCount;
				DedimaniaServerInfo serverInfo = new DedimaniaServerInfo(serverOptions.Name, serverOptions.Comment, serverOptions.Password.Length > 0, string.Empty, 0, plugin.Context.ServerInfo.ServerXMLRpcPort, playersCount, serverOptions.CurrentMaxPlayers, spectatorsCount, serverOptions.CurrentMaxSpectators, serverOptions.CurrentLadderMode, string.Empty);

				if (!plugin.DedimaniaClient.UpdateServerPlayers(plugin.Context.ServerInfo.Version.GetShortName(), currentGameMode.Value, serverInfo, playersToReport.ToArray()))
					plugin.Logger.WarnToUI("Error while calling UpdateServerPlayers!");
			}, "Error in Callbacks_BeginRace Method.", true, plugin.Logger);
        }

        private void Callbacks_BeginRace(object sender, BeginRaceEventArgs e)
        {
            if (e.Erroneous)
            {
                Logger.Error(string.Format("[Callbacks_BeginRace] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
                return;
            }

			RunCatchLog(()=>
			{
				ResetUpdateServerPlayersTimer();
			    ReportCurrentChallenge();
			}, "Error in Callbacks_BeginRace Method.", true);
        }

        private void ReportCurrentChallenge()
        {
            ServerOptions serverOptions = GetServerOptions(this);
            if (serverOptions == null)
                return;

            int? currentGameMode = GetCurrentGameMode(this);
            if (!currentGameMode.HasValue)
                return;

            List<PlayerInfo> playerList = GetPlayerList(this);
            if (playerList == null)
                return;

            ChallengeInfo currentChallenge = GetCurrentChallengeInfo();
            if (currentChallenge == null)
                return;

            List<PlayerInfo> currentPlayers = playerList;

            List<PlayerInfo> nonSpectators = currentPlayers.FindAll(player => !player.IsSpectator);
            List<DedimaniaPlayerInfo> playersToReport = new List<DedimaniaPlayerInfo>();

            foreach (PlayerInfo player in nonSpectators)
            {
                playersToReport.Add(new DedimaniaPlayerInfo(player.Login, string.Empty, string.Empty, player.TeamId, player.IsSpectator, player.LadderRanking, player.IsInOfficialMode));
            }

            int playersCount = playersToReport.Count;
            int spectatorsCount = currentPlayers.Count - playersCount;
            DedimaniaServerInfo serverInfo = new DedimaniaServerInfo(serverOptions.Name, serverOptions.Comment, serverOptions.Password.Length > 0, string.Empty, 0, Context.ServerInfo.ServerXMLRpcPort, playersCount, serverOptions.CurrentMaxPlayers, spectatorsCount, serverOptions.CurrentMaxSpectators, serverOptions.CurrentLadderMode, string.Empty);

            DedimaniaCurrentChallengeReply currentChallengeReply = DedimaniaClient.CurrentChallenge(currentChallenge.UId, currentChallenge.Name, currentChallenge.Environnement, currentChallenge.Author, Context.ServerInfo.Version.GetShortName(), currentGameMode.Value, serverInfo, Convert.ToInt32(Settings.MaxRecordsToReport), playersToReport.ToArray());

            if (currentChallengeReply == null)
                Logger.WarnToUI("Error while calling CurrentChallenge!");
        }

        private void Callbacks_EndRace(object sender, EndRaceEventArgs e)
        {
            if (e.Erroneous)
            {
                Logger.Error(string.Format("[Callbacks_EndRace] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
                return;
            }

			RunCatchLog(()=>
			{
				if (e.Rankings.Count == 0 || !e.Rankings.Exists(ranking => ranking.BestTime != -1))
					return;

				int maxCheckPointAmount = 0;

				foreach (PlayerRank ranking in e.Rankings)
				{
					if (ranking.BestCheckpoints.Count > maxCheckPointAmount)
						maxCheckPointAmount = ranking.BestCheckpoints.Count;
				}

				List<DedimaniaTime> times = new List<DedimaniaTime>();
	            
				foreach (PlayerRank ranking in e.Rankings)
				{
					if (ranking.BestTime >= 6 * 1000)
						times.Add(new DedimaniaTime(ranking.Login, ranking.BestTime, ranking.BestCheckpoints));
				}

				if (times.Count == 0)
					return;

                int? currentGameMode = GetCurrentGameMode(this);
                if (!currentGameMode.HasValue)
                    return;

				ResetUpdateServerPlayersTimer();
                DedimaniaChallengeRaceTimesReply challengeRaceTimesReply = DedimaniaClient.ChallengeRaceTimes(e.Challenge.UId, e.Challenge.Name, e.Challenge.Environnement, e.Challenge.Author, Context.ServerInfo.Version.GetShortName(), currentGameMode.Value, maxCheckPointAmount, Convert.ToInt32(Settings.MaxRecordsToReport), times.ToArray());

				if (challengeRaceTimesReply == null)
					Logger.WarnToUI("Error while calling ChallengeRaceTimes!");
			}, "Error in Callbacks_EndRace Method.", true);
        }

        private static List<PlayerInfo> GetPlayerList(TMSPSPluginBase plugin)
        {
            GenericListResponse<PlayerInfo> playersResponse = plugin.Context.RPCClient.Methods.GetPlayerList();

            if (playersResponse.Erroneous)
            {
                plugin.Logger.Error("Error getting PlayerList: " + playersResponse.Fault.FaultMessage);
                plugin.Logger.ErrorToUI("An error occured during player list retrieval!");
                return null;
            }

            return playersResponse.Value;
        }

        private ChallengeInfo GetCurrentChallengeInfo()
        {
            GenericResponse<ChallengeInfo> currentChallengeInfoResponse = Context.RPCClient.Methods.GetCurrentChallengeInfo();

            if (currentChallengeInfoResponse.Erroneous)
            {
                Logger.Error("Error getting current ChallengeInfo: " + currentChallengeInfoResponse.Fault.FaultMessage);
                Logger.ErrorToUI("An error occured during current challenge info retrieval!");
                return null;
            }

            return currentChallengeInfoResponse.Value;
        }

        private static ServerOptions GetServerOptions(TMSPSPluginBase plugin)
        {
            GenericResponse<ServerOptions> serverOptionsResponse = plugin.Context.RPCClient.Methods.GetServerOptions();

            if (serverOptionsResponse.Erroneous)
            {
                plugin.Logger.Error("Error getting server options: " + serverOptionsResponse.Fault.FaultMessage);
                plugin.Logger.ErrorToUI("An error occured during server options retrieval!");
                return null;
            }

            return serverOptionsResponse.Value;
        }

        private static int? GetCurrentGameMode(TMSPSPluginBase plugin)
        {
            GenericResponse<int> currentGameModeResponse = plugin.Context.RPCClient.Methods.GetGameMode();

            if (currentGameModeResponse.Erroneous)
            {
                plugin.Logger.Error("Error getting current game mode: " + currentGameModeResponse.Fault.FaultMessage);
                plugin.Logger.ErrorToUI("An error occured during current game mode retrieval!");
                return null;
            }

            return currentGameModeResponse.Value;
        }

        private void ResetUpdateServerPlayersTimer()
        {
            if (UpdateServerPlayersTimer != null)
                UpdateServerPlayersTimer.Dispose();

            UpdateServerPlayersTimer = new Timer(UpdateServerPlayers, this, TimeSpan.FromMinutes(UPDATE_SERVER_PLAYERS_INTERVAL_IN_MINUTES), TimeSpan.FromMinutes(UPDATE_SERVER_PLAYERS_INTERVAL_IN_MINUTES));
        }

        #endregion
    }
}