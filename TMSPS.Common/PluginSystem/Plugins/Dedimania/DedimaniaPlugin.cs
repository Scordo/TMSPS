using System;
using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Logging;
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
        private readonly object _rankingModifyLock = new object();

        #endregion

        #region Properties

        public override Version Version { get { return new Version("0.1"); }}
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name{ get { return "DedimaniaPlugin"; } }
        public override string Description { get { return "Saves records in dedimania database."; } }
		public override string ShortName{ get { return "Dedimania"; } }
        protected List<IDedimaniaPluginPlugin> Plugins { get; private set; }
        public DedimaniaSettings Settings { get; private set; }
        public DedimaniaRanking[] Rankings { get; set; }
        public uint? BestTime { get; private set; }

        #endregion

        #region Events

        public event EventHandler<EventArgs<DedimaniaRanking[]>> RankingsChanged;
        public event EventHandler<RankingChangedEventArgs> RankChanged;

        #endregion


        #region Methods

        protected override void Init()
        {
            Rankings = new DedimaniaRanking[] {};
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
            Context.RPCClient.Callbacks.PlayerFinish += Callbacks_PlayerFinish;

            InitializePlugins();
            ResetUpdateServerPlayersTimer();
            ReportCurrentChallenge();
        }

        private void Callbacks_PlayerFinish(object sender, PlayerFinishEventArgs e)
        {
            if (e.Erroneous)
            {
                Logger.Error(string.Format("[Callbacks_PlayerFinish] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
                return;
            }

            RunCatchLog(() =>
            {
                if (e.TimeOrScore > 0)
                {
                    int rankingIndex = Array.FindIndex(Rankings, rank => rank.Login == e.Login);
                    DedimaniaRanking ranking = rankingIndex == -1 ? null : Rankings[rankingIndex];

                    uint? oldRank = null;
                    uint? newRank = null;
                    PlayerInfo playerInfo = null;

                    if (ranking != null)
                    {
                        if (e.TimeOrScore < ranking.TimeOrScore)
                        {
                            oldRank = (uint)rankingIndex + 1;

                            lock (_rankingModifyLock)
                            {
                                ranking.TimeOrScore = Convert.ToUInt32(e.TimeOrScore);
                                List<DedimaniaRanking> newRankings = new List<DedimaniaRanking>(Rankings);
                                newRankings.Sort(DedimaniaRanking.Comparer);
                                newRank = (uint)newRankings.IndexOf(ranking) + 1;

                                Rankings = newRankings.ToArray();
                            }
                        }
                    }
                    else
                    {
                        playerInfo = GetPlayerInfo(e.Login);

                        if (playerInfo == null)
                            return;

                        lock (_rankingModifyLock)
                        {
                            List<DedimaniaRanking> newRankings = new List<DedimaniaRanking>(Rankings);
                            DedimaniaRanking newRanking = new DedimaniaRanking(e.Login, playerInfo.NickName, Convert.ToUInt32(e.TimeOrScore), DateTime.Now);
                            newRankings.Add(newRanking);
                            newRankings.Sort(DedimaniaRanking.Comparer);
                            newRank = Convert.ToUInt32(newRankings.IndexOf(newRanking)) + 1;
                            Rankings = newRankings.ToArray();
                        }
                    }

                    if (newRank.HasValue && newRank <= Settings.MaxRecordsToReport)
                    {
                        if (newRank == 1)
                            BestTime = Convert.ToUInt32(e.TimeOrScore);

                        OnRankingsChanged(Rankings);

                        playerInfo = playerInfo ?? GetPlayerInfo(e.Login);

                        if (playerInfo == null)
                            return;

                        OnRankChanged(newRank.Value, oldRank, e.Login, playerInfo.NickName, Convert.ToUInt32(e.TimeOrScore));
                    }
                }
            }, "Error in Callbacks_PlayerFinish Method.", true);
        }

        private void OnRankingsChanged(DedimaniaRanking[] rankings)
        {
            if (RankingsChanged != null)
                RankingsChanged(this, new EventArgs<DedimaniaRanking[]>(rankings));
        }

        private void OnRankChanged(uint newRank, uint? oldRank, string login, string nickname, uint timeOrScore)
        {
            if (RankChanged != null)
                RankChanged(this,new RankingChangedEventArgs(newRank, oldRank, login, nickname, timeOrScore));
        }

        private void InitializePlugins()
        {
            Plugins = Settings.GetPlugins(Logger);

            foreach (IDedimaniaPluginPlugin plugin in Plugins)
            {
                plugin.ProvideHostPlugin(this);
                plugin.InitPlugin(Context, new ConsoleUILogger("TMSPS", string.Format(" - [{0}]", plugin.ShortName)));
            }
        }

        private void DisposePlugins(bool connectionLost)
        {
            Plugins.ForEach(plugin => plugin.DisposePlugin(connectionLost));
        }

        protected override void Dispose(bool connectionLost)
        {
            DisposePlugins(connectionLost);
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

            if (currentChallengeReply != null)
            {
                FillRankingsFromDedimania(currentChallengeReply.Records);
                BestTime = Rankings.Length == 0 ? null : (uint?) Rankings[0].TimeOrScore;
                OnRankingsChanged(Rankings);
            }
            else
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

        public static List<PlayerInfo> GetPlayerList(TMSPSPluginBase plugin)
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

        public PlayerInfo GetPlayerInfo(string login)
        {
            GenericResponse<PlayerInfo> playerInfoResponse = Context.RPCClient.Methods.GetPlayerInfo(login);

            if (playerInfoResponse.Erroneous)
            {
                Logger.Error(string.Format("Error getting Playerinfo for player with login {0}: {1}", login, playerInfoResponse.Fault.FaultMessage));
                Logger.ErrorToUI(string.Format("Error getting Playerinfo for player with login {0}", login));
                return null;
            }

            return playerInfoResponse.Value;
        }

        private void FillRankingsFromDedimania(IEnumerable<DedimaniaRecord> records)
        {
            List<DedimaniaRanking> rankings = new List<DedimaniaRanking>();

            foreach (DedimaniaRecord dedimaniaRecord in records)
            {
                rankings.Add(new DedimaniaRanking(dedimaniaRecord.Login, dedimaniaRecord.Nickname, Convert.ToUInt32(dedimaniaRecord.BestTime.TotalMilliseconds), DateTime.MinValue.AddSeconds(dedimaniaRecord.Rank)));
            }

            rankings.Sort(DedimaniaRanking.Comparer);
            Rankings = rankings.ToArray();
        }

        #endregion
    }

    public class RankingChangedEventArgs : EventArgs
    {
        #region Properties

        public uint NewRank { get; private set; }
        public uint? OldRank { get; private set; }
        public string Login { get; private set; }
        public string Nickname { get; private set; }
        public uint TimeOrScore { get; private set; }
        public bool RankChanged { get { return NewRank != OldRank; } }

        #endregion

        #region Constructors

        public RankingChangedEventArgs(uint newRank, uint? oldRank, string login, string nickname, uint timeOrScore)
        {
            NewRank = newRank;
            OldRank = oldRank;
            Login = login;
            Nickname = nickname;
            TimeOrScore = timeOrScore;
        }

        #endregion
    }
}