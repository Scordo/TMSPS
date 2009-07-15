using System;
using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Logging;
using TMSPS.Core.PluginSystem.Configuration;
using TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication;
using Timer=System.Threading.Timer;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania
{
    public class DedimaniaPlugin : TMSPSPlugin
    {
        enum AuthResult { Failed, Erroneous, Success }

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

        public override Version Version { get { return new Version("0.3"); }}
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name{ get { return "DedimaniaPlugin"; } }
        public override string Description { get { return "Saves records in dedimania database."; } }
		public override string ShortName{ get { return "Dedimania"; } }
        protected List<IDedimaniaPluginPlugin> Plugins { get; private set; }
        public DedimaniaSettings Settings { get; private set; }
        public DedimaniaRanking[] Rankings { get; set; }
        public uint? BestTime { get; private set; }
        public bool IsDedimaniaResponsive { get; private set; }


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

            InitializeDedimaniaClient();

            AuthResult authResult = TryAuthentication();
            if (authResult == AuthResult.Failed)
                return;

            IsDedimaniaResponsive = (authResult == AuthResult.Success);

            InitializePlugins();
            ResetUpdateServerPlayersTimer();

            DedimaniaClient.Url = Settings.ReportUrl;

            if (IsDedimaniaResponsive)
                ReportCurrentChallenge(GetExistingCurrentRankings(), GetCurrentChallengeInfoCached());
            else
                OnRankingsChanged(new DedimaniaRanking[] {});

            Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerFinish += Callbacks_PlayerFinish;
        }

        private void InitializeDedimaniaClient()
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

            // set default timeout of dedimania requests to 10 seconds
            DedimaniaClient = new DedimaniaClient(Settings.AuthUrl, authParams){Timeout = 10000};
        }

        private AuthResult TryAuthentication()
        {
            try
            {
                if (!DedimaniaClient.Authenticate())
                {
                    Logger.ErrorToUI("Authentication failed. Stopping Plugin!");
                    return AuthResult.Failed;
                }
    
                return AuthResult.Success;
            }
            catch(Exception ex)
            {
                Logger.ErrorToUI("Error starting dedimania plugin: " + ex.Message);
                Logger.Error("Error starting dedimania plugin: " + ex);

                return AuthResult.Erroneous;
            }
        }

        private List<PlayerRank> GetExistingCurrentRankings()
        {
            List<PlayerRank> currentRankings = GetCurrentRanking() ?? new List<PlayerRank>();
            List<PlayerSettings> playerList = Context.PlayerSettings.GetAllAsList();

            foreach (PlayerRank rank in currentRankings.ToArray())
            {
                if (!playerList.Exists(p => p.Login == rank.Login))
                    currentRankings.Remove(rank);
            }

            return currentRankings;
        }

        protected override void Dispose(bool connectionLost)
        {
            DisposePlugins(connectionLost);

            Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
            Context.RPCClient.Callbacks.PlayerFinish -= Callbacks_PlayerFinish;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
        }

        private void Callbacks_PlayerFinish(object sender, PlayerFinishEventArgs e)
        {
            if (!IsDedimaniaResponsive)
                return;

            RunCatchLog(() =>
            {
                if (e.TimeOrScore > 0)
                {
                    int rankingIndex = Array.FindIndex(Rankings, rank => rank.Login == e.Login);
                    DedimaniaRanking ranking = rankingIndex == -1 ? null : Rankings[rankingIndex];

                    uint? oldRank = null;
                    uint? newRank = null;
                    string nickname = null;

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
                        nickname = GetNickname(e.Login);

                        if (nickname == null)
                            return;

                        lock (_rankingModifyLock)
                        {
                            List<DedimaniaRanking> newRankings = new List<DedimaniaRanking>(Rankings);
                            DedimaniaRanking newRanking = new DedimaniaRanking(e.Login, nickname, Convert.ToUInt32(e.TimeOrScore), DateTime.Now);
                            newRankings.Add(newRanking);
                            newRankings.Sort(DedimaniaRanking.Comparer);
                            newRank = Convert.ToUInt32(newRankings.IndexOf(newRanking)) + 1;
                            Rankings = newRankings.ToArray();
                        }
                    }

                    if (newRank.HasValue && newRank <= DedimaniaSettings.MAX_RECORDS_TO_REPORT)
                    {
                        if (newRank == 1)
                            BestTime = Convert.ToUInt32(e.TimeOrScore);

                        OnRankingsChanged(Rankings);

                        nickname = nickname ?? GetNickname(e.Login);

                        if (nickname == null)
                            return;

                        OnRankChanged(newRank.Value, oldRank, e.Login, nickname, Convert.ToUInt32(e.TimeOrScore));
                    }
                }
            }, "Error in Callbacks_PlayerFinish Method.", true);
        }

        private void Callbacks_BeginRace(object sender, BeginRaceEventArgs e)
        {
            RunCatchLog(() =>
            {
                ReportCurrentChallenge(null, e.ChallengeInfo);

                if (IsDedimaniaResponsive)
                    ResetUpdateServerPlayersTimer();
            }, "Error in Callbacks_BeginChallenge Method.", true);
        }

        private void Callbacks_EndRace(object sender, EndRaceEventArgs e)
        {
            RunCatchLog(() =>
            {
                if (e.Rankings.Count == 0 || !e.Rankings.Exists(ranking => ranking.BestTime != -1))
                    return;

                List<DedimaniaTime> times = new List<DedimaniaTime>();

                foreach (PlayerRank ranking in e.Rankings)
                {
                    if (ranking.BestTime >= 6 * 1000 && CheckpointsValid(ranking.BestCheckpoints))
                        times.Add(new DedimaniaTime(ranking.Login, ranking.BestTime, ranking.BestCheckpoints));
                }

                if (times.Count == 0)
                    return;

                GameMode? currentGameMode = GetCurrentGameModeCached(this);
                if (!currentGameMode.HasValue)
                    return;

                ResetUpdateServerPlayersTimer();
                DedimaniaChallengeRaceTimesReply challengeRaceTimesReply = DedimaniaClient.ChallengeRaceTimes(e.Challenge.UId, e.Challenge.Name, e.Challenge.Environnement, e.Challenge.Author, Context.ServerInfo.Version.GetShortName(), (int)currentGameMode.Value, e.Challenge.NumberOfCheckpoints, (int)DedimaniaSettings.MAX_RECORDS_TO_REPORT, times.ToArray());

                if (challengeRaceTimesReply == null)
                    Logger.WarnToUI("Error while calling ChallengeRaceTimes!");
            }, "Error in Callbacks_EndChallenge Method.", true);
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

        private static void UpdateServerPlayers(object state)
        {
            DedimaniaPlugin plugin = (DedimaniaPlugin) state;
            RunCatchLog(()=>
            {
                ServerOptions serverOptions = GetServerOptionsCached(plugin);
                if (serverOptions == null)
                    return;

                GameMode? currentGameMode = GetCurrentGameModeCached(plugin);
                if (!currentGameMode.HasValue)
                    return;

                List<PlayerSettings> currentPlayers = plugin.Context.PlayerSettings.GetAllAsList();

                List<PlayerSettings> nonSpectators = currentPlayers.FindAll(player => !player.SpectatorStatus.IsSpectator);
                List<DedimaniaPlayerInfo> playersToReport = new List<DedimaniaPlayerInfo>();

                foreach (PlayerSettings playerSettings in nonSpectators)
                {
                    playersToReport.Add(new DedimaniaPlayerInfo(playerSettings.Login, string.Empty, string.Empty, playerSettings.TeamID, playerSettings.SpectatorStatus.IsSpectator, playerSettings.LadderRanking, playerSettings.IsInOfficialMode));
                }

                int playersCount = playersToReport.Count;
                int spectatorsCount = currentPlayers.Count - playersCount;
                DedimaniaServerInfo serverInfo = new DedimaniaServerInfo(serverOptions.Name, serverOptions.Comment, serverOptions.Password.Length > 0, string.Empty, 0, plugin.Context.ServerInfo.ServerXMLRpcPort, playersCount, serverOptions.CurrentMaxPlayers, spectatorsCount, serverOptions.CurrentMaxSpectators, serverOptions.CurrentLadderMode, string.Empty);

                if (!plugin.DedimaniaClient.UpdateServerPlayers(plugin.Context.ServerInfo.Version.GetShortName(), (int) currentGameMode.Value, serverInfo, playersToReport.ToArray()))
                    plugin.Logger.WarnToUI("Error while calling UpdateServerPlayers!");
            }, "Error in Callbacks_BeginRace Method.", true, plugin.Logger);
        }

        private void ReportCurrentChallenge(ICollection<PlayerRank> currentRankings, ChallengeListSingleInfo currentChallenge)
        {
            if (currentChallenge == null)
                return;

            ServerOptions serverOptions = GetServerOptionsCached(this);
            if (serverOptions == null)
                return;

            GameMode? currentGameMode = GetCurrentGameModeCached(this);
            if (!currentGameMode.HasValue)
                return;

            List<PlayerSettings> currentPlayers = Context.PlayerSettings.GetAllAsList();

            List<PlayerSettings> nonSpectators = currentPlayers.FindAll(player => !player.SpectatorStatus.IsSpectator);
            List<DedimaniaPlayerInfo> playersToReport = new List<DedimaniaPlayerInfo>();

            foreach (PlayerSettings playerSettings in nonSpectators)
            {
                playersToReport.Add(new DedimaniaPlayerInfo(playerSettings.Login, string.Empty, string.Empty, playerSettings.TeamID, playerSettings.SpectatorStatus.IsSpectator, playerSettings.LadderRanking, playerSettings.IsInOfficialMode));
            }

            int playersCount = playersToReport.Count;
            int spectatorsCount = currentPlayers.Count - playersCount;
            DedimaniaServerInfo serverInfo = new DedimaniaServerInfo(serverOptions.Name, serverOptions.Comment, serverOptions.Password.Length > 0, string.Empty, 0, Context.ServerInfo.ServerXMLRpcPort, playersCount, serverOptions.CurrentMaxPlayers, spectatorsCount, serverOptions.CurrentMaxSpectators, serverOptions.CurrentLadderMode, string.Empty);

            DedimaniaCurrentChallengeReply currentChallengeReply = null;

            try
            {
                currentChallengeReply = DedimaniaClient.CurrentChallenge(currentChallenge.UId, currentChallenge.Name, currentChallenge.Environnement, currentChallenge.Author, Context.ServerInfo.Version.GetShortName(), (int)currentGameMode.Value, serverInfo, (int)DedimaniaSettings.MAX_RECORDS_TO_REPORT, playersToReport.ToArray());
                IsDedimaniaResponsive = true;
            }
            catch (Exception ex)
            {
                Logger.ErrorToUI("Could not report current challenge: " + ex.Message );
                Logger.Error("Could not report current challenge: " + ex);
                IsDedimaniaResponsive = false;
            }

            if (currentChallengeReply != null)
            {
                FillRankingsFromDedimania(currentChallengeReply.Records, currentRankings);
                BestTime = Rankings.Length == 0 ? null : (uint?) Rankings[0].TimeOrScore;
            }
            else 
            {
                Rankings = new DedimaniaRanking[] {};
                BestTime = null;

                if (IsDedimaniaResponsive)
                    Logger.Debug("Error while calling CurrentChallenge!");
            }

            OnRankingsChanged(Rankings);
        }

        private void ResetUpdateServerPlayersTimer()
        {
            if (UpdateServerPlayersTimer != null)
                UpdateServerPlayersTimer.Dispose();

            UpdateServerPlayersTimer = new Timer(UpdateServerPlayers, this, TimeSpan.FromMinutes(UPDATE_SERVER_PLAYERS_INTERVAL_IN_MINUTES), TimeSpan.FromMinutes(UPDATE_SERVER_PLAYERS_INTERVAL_IN_MINUTES));
        }

        private void FillRankingsFromDedimania(IEnumerable<DedimaniaRecord> records, ICollection<PlayerRank> currentRankings)
        {
            List<DedimaniaRanking> rankings = new List<DedimaniaRanking>();

            foreach (DedimaniaRecord dedimaniaRecord in records)
            {
                rankings.Add(new DedimaniaRanking(dedimaniaRecord.Login, dedimaniaRecord.Nickname, Convert.ToUInt32(dedimaniaRecord.BestTime.TotalMilliseconds), DateTime.MinValue.AddSeconds(dedimaniaRecord.Rank)));
            }

            if (currentRankings != null)
            {
                foreach (PlayerRank playerRank in currentRankings)
                {
                    if (playerRank.BestTime <= 0)
                        continue;

                    string nickname = GetNickname(playerRank.Login);

                    DedimaniaRanking existingRanking = rankings.Find(ranking => ranking.Login == playerRank.Login);

                    if (existingRanking != null)
                    {
                        if (existingRanking.TimeOrScore != playerRank.BestTime)
                            existingRanking.Created = DateTime.Now.AddMilliseconds(-1 * currentRankings.Count).AddMilliseconds(playerRank.Rank);

                        existingRanking.TimeOrScore = Convert.ToUInt32(playerRank.BestTime);
                    }
                    else
                    {
                        rankings.Add(new DedimaniaRanking(playerRank.Login, nickname, Convert.ToUInt32(playerRank.BestTime), DateTime.Now.AddMilliseconds(-1 * currentRankings.Count).AddMilliseconds(playerRank.Rank)));
                    }
                }
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