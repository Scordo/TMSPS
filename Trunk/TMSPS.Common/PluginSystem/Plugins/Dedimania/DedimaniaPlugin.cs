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
    public class DedimaniaPlugin : TMSPSPluginBase
    {
        #region Constants

        private const int UPDATE_SERVER_PLAYERS_INTERVAL_IN_MINUTES = 4;
        private const int MAX_AMOUNT_OF_RECORDS_TO_RECEIVE = 30;
        private const string FIRST_AUTH_URL = "http://dedimania.net/RPC4/server.php";
        private const string COMMANDS_URL = "http://dedimania.net:8015/Dedimania";
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

		public override string ShortNameForLogging
		{
			get { return "DedimaniaPlugin"; }
		}

        #endregion

        #region Methods

        protected override void Init()
        {
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

            DedimaniaClient = new DedimaniaClient(FIRST_AUTH_URL, authParams);

            if (!DedimaniaClient.Authenticate())
            {
				Logger.ErrorToUI("Auth failed. Stopping Plugin!");
                return;
            }

            DedimaniaClient.Url = COMMANDS_URL;
            Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;

            ResetUpdateServerPlayersTimer();
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
				GenericResponse<ServerOptions> serverOptionsResponse = plugin.Context.RPCClient.Methods.GetServerOptions();

				if (serverOptionsResponse.Erroneous)
				{
					plugin.Logger.WarnToUI("Error while retrieving server options:"+serverOptionsResponse.Fault.FaultMessage);
					return;
				}

				GenericResponse<int> currentGameModeResponse = plugin.Context.RPCClient.Methods.GetGameMode();

				if (currentGameModeResponse.Erroneous)
				{
					plugin.Logger.WarnToUI("Error while retrieving current game mode:" + currentGameModeResponse.Fault.FaultMessage);
					return;
				}

				GenericListResponse<PlayerInfo> playersResponse = plugin.Context.RPCClient.Methods.GetPlayerList();

				if (playersResponse.Erroneous)
				{
					plugin.Logger.WarnToUI("Error while retrieving player list:" + playersResponse.Fault.FaultMessage);
					return;
				}

				ServerOptions serverOptions = serverOptionsResponse.Value;
				List<PlayerInfo> currentPlayers = playersResponse.Value;

				List<PlayerInfo> nonSpectators = currentPlayers.FindAll(player => !player.IsSpectator);
				List<DedimaniaPlayerInfo> playersToReport = new List<DedimaniaPlayerInfo>();

				foreach (PlayerInfo player in nonSpectators)
				{
					playersToReport.Add(new DedimaniaPlayerInfo(player.Login, string.Empty, string.Empty, player.TeamId, player.IsSpectator, player.LadderRanking, player.IsInOfficialMode));
				}

				int playersCount = playersToReport.Count;
				int spectatorsCount = currentPlayers.Count - playersCount;
				DedimaniaServerInfo serverInfo = new DedimaniaServerInfo(serverOptions.Name, serverOptions.Comment, serverOptions.Password.Length > 0, string.Empty, 0, plugin.Context.ServerInfo.ServerXMLRpcPort, playersCount, serverOptions.CurrentMaxPlayers, spectatorsCount, serverOptions.CurrentMaxSpectators, serverOptions.CurrentLadderMode, string.Empty);

				if (!plugin.DedimaniaClient.UpdateServerPlayers(plugin.Context.ServerInfo.Version.GetShortName(), currentGameModeResponse.Value, serverInfo, playersToReport.ToArray()))
					plugin.Logger.WarnToUI("Error while calling UpdateServerPlayers!");
			}, "Error in Callbacks_BeginRace Method.", true, plugin.Logger);
        }

        private void Callbacks_BeginRace(object sender, BeginRaceEventArgs e)
        {
			RunCatchLog(()=>
			{
				ResetUpdateServerPlayersTimer();

				GenericResponse<ServerOptions> serverOptionsResponse = Context.RPCClient.Methods.GetServerOptions();

				if (serverOptionsResponse.Erroneous)
				{
					Logger.WarnToUI("Error while retrieving server options:" + serverOptionsResponse.Fault.FaultMessage);
					return;
				}

				GenericResponse<int> currentGameModeResponse = Context.RPCClient.Methods.GetGameMode();

				if (currentGameModeResponse.Erroneous)
				{
					Logger.WarnToUI("Error while retrieving current game mode:" + currentGameModeResponse.Fault.FaultMessage);
					return;
				}

				GenericListResponse<PlayerInfo> playersResponse = Context.RPCClient.Methods.GetPlayerList();

				if (playersResponse.Erroneous)
				{
					Logger.WarnToUI("Error while retrieving player list:" + playersResponse.Fault.FaultMessage);
					return;
				}

				ServerOptions serverOptions = serverOptionsResponse.Value;
				List<PlayerInfo> currentPlayers = playersResponse.Value;

				List<PlayerInfo> nonSpectators = currentPlayers.FindAll(player => !player.IsSpectator);
				List<DedimaniaPlayerInfo> playersToReport = new List<DedimaniaPlayerInfo>();

				foreach (PlayerInfo player in nonSpectators)
				{
					playersToReport.Add(new DedimaniaPlayerInfo(player.Login, string.Empty, string.Empty, player.TeamId, player.IsSpectator, player.LadderRanking, player.IsInOfficialMode));
				}

				int playersCount = playersToReport.Count;
				int spectatorsCount = currentPlayers.Count - playersCount;
				DedimaniaServerInfo serverInfo = new DedimaniaServerInfo(serverOptions.Name, serverOptions.Comment, serverOptions.Password.Length > 0, string.Empty, 0, Context.ServerInfo.ServerXMLRpcPort, playersCount, serverOptions.CurrentMaxPlayers, spectatorsCount, serverOptions.CurrentMaxSpectators, serverOptions.CurrentLadderMode, string.Empty);

				DedimaniaCurrentChallengeReply currentChallengeReply = DedimaniaClient.CurrentChallenge(e.ChallengeInfo.UId, e.ChallengeInfo.Name, e.ChallengeInfo.Environnement, e.ChallengeInfo.Author, Context.ServerInfo.Version.GetShortName(), currentGameModeResponse.Value, serverInfo, MAX_AMOUNT_OF_RECORDS_TO_RECEIVE, playersToReport.ToArray());

				if (currentChallengeReply == null)
					Logger.WarnToUI("Error while calling CurrentChallenge!");
			}, "Error in Callbacks_BeginRace Method.", true);
        }

        private void Callbacks_EndRace(object sender, EndRaceEventArgs e)
        {
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

				GenericResponse<int> currentGameModeResponse = Context.RPCClient.Methods.GetGameMode();

				if (currentGameModeResponse.Erroneous)
				{
					Logger.WarnToUI("Error while retrieving current game mode:" + currentGameModeResponse.Fault.FaultMessage);
					return;
				}

				ResetUpdateServerPlayersTimer();
				DedimaniaChallengeRaceTimesReply challengeRaceTimesReply = DedimaniaClient.ChallengeRaceTimes(e.Challenge.UId, e.Challenge.Name, e.Challenge.Environnement, e.Challenge.Author, Context.ServerInfo.Version.GetShortName(), currentGameModeResponse.Value, maxCheckPointAmount, MAX_AMOUNT_OF_RECORDS_TO_RECEIVE, times.ToArray());

				if (challengeRaceTimesReply == null)
					Logger.WarnToUI("Error while calling ChallengeRaceTimes!");
			}, "Error in Callbacks_EndRace Method.", true);
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