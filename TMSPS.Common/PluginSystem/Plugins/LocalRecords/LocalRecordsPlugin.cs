using System;
using System.Collections.Generic;
using System.Timers;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.Logging;
using Version=System.Version;
using System.Linq;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class LocalRecordsPlugin : TMSPSPlugin
	{
	    #region Properties

	    public override Version Version { get { return new Version("1.0.0.0"); } }
	    public override string Author { get { return "Jens Hofmann"; } }
	    public override string Name{ get { return "Local Records Plugin"; } }
	    public override string Description { get { return "Saves records and statistics in a local database."; } }
	    public override string ShortName { get { return "LocalRecords"; } }

	    public IAdapterProvider AdapterProvider { get; protected set; }
	    public IChallengeAdapter ChallengeAdapter { get; protected set; }
	    public IPlayerAdapter PlayerAdapter { get; protected set; }
	    public IPositionAdapter PositionAdapter { get; protected set; }
	    public IRecordAdapter RecordAdapter { get; protected set; }
	    public IRatingAdapter RatingAdapter { get; protected set; }
	    public ISessionAdapter SessionAdapter { get; protected set; }
	    public int CurrentChallengeID { get; protected set; }
	    protected Timer TimePlayedTimer { get; private set; }
	    public Dictionary<string, PlayerInfo> PlayerInfoCache { get; protected set; }
	    public LocalRecordsSettings Settings { get; protected set; }
	    protected List<ILocalRecordsPluginPlugin> Plugins {get; private set;}
        protected List<RankEntry> LocalRecords { get; private set; }

	    #endregion

	    #region Events

	    public event EventHandler<PlayerVoteEventArgs> PlayerVoted;
	    public event EventHandler<PlayerNewRecordEventArgs> PlayerNewRecord;
	    public event EventHandler<PlayerWinEventArgs> PlayerWins;
	    public event EventHandler<PlayerCreatedOrUpdatedEventArgs> PlayerCreatedOrUpdated;
	    public event EventHandler<ChallengeCreatedOrUpdatedEventArgs> ChallengeCreatedOrUpdated;
        public event EventHandler<EventArgs<RankEntry[]>> LocalRecordsDetermined;

	    #endregion

	    #region Methods

	    protected override void Init()
	    {
	        Logger.InfoToUI("Started initialziation of " + ShortName);
	        PlayerInfoCache = new Dictionary<string, PlayerInfo>();
	    	
	        Settings = LocalRecordsSettings.ReadFromFile(PluginSettingsFilePath);
	        
	        try
	        {
	            AdapterProvider = AdapterProviderFactory.GetAdapterProvider(Settings);
	            ChallengeAdapter = AdapterProvider.GetChallengeAdapter();
	            PlayerAdapter = AdapterProvider.GetPlayerAdapter();
	            PositionAdapter = AdapterProvider.GetPositionAdapter();
	            RecordAdapter = AdapterProvider.GetRecordAdapter();
	            RatingAdapter = AdapterProvider.GetRatingAdapter();
	            SessionAdapter = AdapterProvider.GetSessionAdapter();
	        }
	        catch (Exception ex)
	        {
	            Logger.Error("Error initializing AdapterProvider for local records.", ex);
	            Logger.ErrorToUI(string.Format("An error occured. {0} not started!", Name));
	            return;
	        }

	        List<PlayerInfo> players = GetPlayerList();
	        if (players == null)
	            return;

	        foreach (PlayerInfo playerInfo in players)
	        {
	            PlayerAdapter.CreateOrUpdate(new Player(playerInfo.Login, playerInfo.NickName));
	        }

	        ChallengeInfo currentChallengeInfo = GetCurrentChallengeInfo();
	        if (currentChallengeInfo == null)
	            return;

	        EnsureChallengeExistsInStorage(currentChallengeInfo);
	        
	        TimePlayedTimer = new Timer(30000);
	        TimePlayedTimer.Elapsed += TimePlayedTimer_Elapsed;
	        TimePlayedTimer.Start();

            DetermineLocalRecords();

            InitializePlugins();
            OnLocalRecordsDetermined(LocalRecords);

	        Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
	        Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
	        Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
	        Context.RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
	        Context.RPCClient.Callbacks.PlayerFinish += Callbacks_PlayerFinish;
	        Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;

	        Logger.InfoToUI("Finished initialization of " + ShortName);
	    }

	    private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
	    {
	        if (e.Erroneous)
	        {
	            Logger.Error(string.Format("[Callbacks_PlayerChat] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
	            return;
	        }

	        RunCatchLog(()=>
            {
                if (e.IsServerMessage || e.Text.IsNullOrTimmedEmpty() || e.IsRegisteredCommand)
                    return;

                string message = e.Text.Trim();
                ushort? voteValue = null;

                switch (message)
                {
                    case "++":
                        voteValue = 8;
                        break;
                    case "--":
                        voteValue = 0;
                        break;
                    case "+-":
                    case "-+":
                        voteValue = 4;
                        break;
                    case "+0":
                    case "+1":
                    case "+2":
                    case "+3":
                    case "+4":
                    case "+5":
                    case "+6":
                    case "+7":
                    case "+8":
                        voteValue = Convert.ToUInt16(message.Substring(1));
                        break;
                }

                if (voteValue.HasValue)
                {
                    double? averageVote = RatingAdapter.Vote(e.Login, CurrentChallengeID, voteValue.Value);

                    if (averageVote.HasValue)
                        OnPlayerVoted(e.Login, CurrentChallengeID, voteValue.Value, averageVote.Value);
                }
            }, "Error in Callbacks_PlayerChat Method.", true);
	    }

	    private void Callbacks_PlayerFinish(object sender, PlayerFinishEventArgs e)
	    {
	        if (e.Erroneous)
	        {
	            Logger.Error(string.Format("[Callbacks_PlayerFinish] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
	            return;
	        }

	        RunCatchLog(()=>
            {
                if (e.TimeOrScore > 0)
                {
                    uint? oldPosition, newPosition;
                    bool newBest;
                    RecordAdapter.CheckAndWriteNewRecord(e.Login, CurrentChallengeID, e.TimeOrScore, out oldPosition, out newPosition, out newBest);

                    if (newBest)
                    {
                        PlayerInfo playerInfo = GetPlayerInfo(e.Login, true);

                        if (playerInfo != null && newPosition <= Settings.MaxRecordsToReport)
                            OnPlayerNewRecord(playerInfo, e.TimeOrScore, oldPosition, newPosition);
                    }

                    SessionAdapter.AddSession(e.Login, CurrentChallengeID, e.TimeOrScore);
                }
            }, "Error in Callbacks_PlayerFinish Method.", true);
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
                if (e.Rankings.Count > 1)
                {
                    // there must be at least 2 players to increase the wins for the first player
                    if (e.Rankings[0].BestTime > 0)
                    {
                        uint wins = PlayerAdapter.IncreaseWins(e.Rankings[0].Login);
                        OnPlayerWins(e.Rankings[0], wins);
                        int maxRank = e.Rankings.Max(playerRank => playerRank.Rank);

                        foreach (PlayerRank playerRank in e.Rankings)
                        {
                            PositionAdapter.AddPosition(playerRank.Login, e.Challenge.UId, Convert.ToUInt16(playerRank.Rank), Convert.ToUInt16(maxRank));
                        }
                    }
                }
            }, "Error in Callbacks_EndRace Method.", true);
	    }

	    private void TimePlayedTimer_Elapsed(object sender, ElapsedEventArgs e)
	    {
	        RunCatchLog(UpdateTimePlayedForAllCurrentPlayers, "Error in TimePlayedTimer_Elapsed Method.", true);
	    }

	    private void Callbacks_PlayerDisconnect(object sender,PlayerDisconnectEventArgs e)
	    {
	        if (e.Erroneous)
	        {
	            Logger.Error(string.Format("[Callbacks_PlayerDisconnect] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
	            return;
	        }

	        RunCatchLog(() => PlayerAdapter.UpdateTimePlayed(e.Login), "Error in Callbacks_PlayerDisconnect Method.", true);
	    }

	    private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
	    {
	        if (e.Erroneous)
	        {
	            Logger.Error(string.Format("[Callbacks_PlayerConnect] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
	            return;
	        }

	        RunCatchLog(()=>
            {
                PlayerInfo playerInfo = GetPlayerInfo(e.Login);

                if (playerInfo == null)
                    return;

                Player player = new Player(playerInfo.Login, playerInfo.NickName);
                PlayerAdapter.CreateOrUpdate(player);
                OnPlayerCreatedOrUpdated(player, playerInfo);
            }, "Error in Callbacks_PlayerConnect Method.", true);
	    }

	    private void Callbacks_BeginRace(object sender, BeginRaceEventArgs e)
	    {
	        if (e.Erroneous)
	        {
	            Logger.Error(string.Format("[Callbacks_BeginRace] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
	            return;
	        }

	        RunCatchLog(() =>
            {
                EnsureChallengeExistsInStorage(e.ChallengeInfo);
                DetermineLocalRecords();
                OnLocalRecordsDetermined(LocalRecords);
            }, "Error in Callbacks_BeginRace Method.", true);
	    }

	    private void EnsureChallengeExistsInStorage(ChallengeListSingleInfo challengeInfo)
	    {
	        Challenge challenge = new Challenge(challengeInfo.UId, challengeInfo.Name, challengeInfo.Author, challengeInfo.Environnement);
	        ChallengeAdapter.IncreaseRaces(challenge);
	        CurrentChallengeID = challenge.ID.Value;

	        OnChallengeCreatedOrUpdated(challengeInfo, challenge);
	    }

	    public PlayerInfo GetPlayerInfo(string login, bool allowCached)
	    {
	        if (!allowCached || !PlayerInfoCache.ContainsKey(login))
	            return GetPlayerInfo(login);

	        return PlayerInfoCache[login];
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

	        // cache the playerinfo
	        PlayerInfoCache[playerInfoResponse.Value.Login] = playerInfoResponse.Value;

	        return playerInfoResponse.Value;
	    }

	    public List<PlayerInfo> GetPlayerList()
	    {
	        GenericListResponse<PlayerInfo> playersResponse = Context.RPCClient.Methods.GetPlayerList();

	        if (playersResponse.Erroneous)
	        {
	            Logger.Error("Error getting PlayerList: " + playersResponse.Fault.FaultMessage);
	            Logger.ErrorToUI("An error occured during player list retrieval!");
	            return null;
	        }

	        // cache the playerInfo list
	        playersResponse.Value.ForEach(info => PlayerInfoCache[info.Login] = info);

	        return playersResponse.Value;
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

	        return currentChallengeInfoResponse.Value;
	    }

	    private void UpdateTimePlayedForAllCurrentPlayers()
	    {
	        List<PlayerInfo> players = GetPlayerList();

	        if (players != null)
	        {
	            foreach (PlayerInfo playerInfo in players)
	            {
	                PlayerAdapter.UpdateTimePlayed(playerInfo.Login);
	            }    
	        }
	    }

	    private void InitializePlugins()
	    {
	        Plugins = Settings.GetPlugins(Logger);

	        foreach (ILocalRecordsPluginPlugin plugin in Plugins)
	        {
	            plugin.ProvideHostPlugin(this);
	            plugin.InitPlugin(Context, new ConsoleUILogger("TMSPS", string.Format(" - [{0}]", plugin.ShortName)));
	        }
	    }

        private void DetermineLocalRecords()
        {
            LocalRecords = RecordAdapter.GetTopRecordsForChallenge(CurrentChallengeID, Settings.MaxRecordsToReport);
            Context.ValueStore.SetOrUpdate(GlobalConstants.LOCAL_RECORDS, LocalRecords.ToArray());
            Context.ValueStore.SetOrUpdate(GlobalConstants.FIRST_LOCAL_RECORD_TIMEORSCORE, LocalRecords.Count == 0 ? null : (int?)LocalRecords[0].TimeOrScore);
        }

	    private void DisposePlugins()
	    {
	        foreach (ILocalRecordsPluginPlugin plugin in Plugins)
	        {
	            plugin.DisposePlugin();
	        }
	    }

	    protected override void Dispose()
	    {
	        TimePlayedTimer.Stop();
	        UpdateTimePlayedForAllCurrentPlayers();
	        DisposePlugins();

	        Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
	        Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
	        Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
	        Context.RPCClient.Callbacks.PlayerDisconnect -= Callbacks_PlayerDisconnect;
	        Context.RPCClient.Callbacks.PlayerFinish -= Callbacks_PlayerFinish;
	    }

	    protected void OnPlayerVoted(string login, int challengeID, ushort voteValue, double averageVoteValue)
	    {
	        if (PlayerVoted != null)
	            PlayerVoted(this, new PlayerVoteEventArgs(login, challengeID, voteValue, averageVoteValue));
	    }

	    protected void OnPlayerNewRecord(PlayerInfo playerInfo, int timeOrScore, uint? oldPosition, uint? newPosition)
	    {
	        if (PlayerNewRecord != null)
	            PlayerNewRecord(this, new PlayerNewRecordEventArgs(playerInfo, timeOrScore, oldPosition, newPosition) );
	    }

	    protected void OnPlayerWins(PlayerRank rankingInfo, uint wins)
	    {
	        if (PlayerWins != null)
	            PlayerWins(this, new PlayerWinEventArgs(rankingInfo, wins));
	    }

	    protected void OnPlayerCreatedOrUpdated(Player player, PlayerInfo playerInfo)
	    {
	        if (PlayerCreatedOrUpdated != null)
	            PlayerCreatedOrUpdated(this, new PlayerCreatedOrUpdatedEventArgs(player, playerInfo));
	    }

	    protected void OnChallengeCreatedOrUpdated(ChallengeListSingleInfo challengeInfo, Challenge challenge)
	    {
	        if (ChallengeCreatedOrUpdated != null)
	            ChallengeCreatedOrUpdated(this, new ChallengeCreatedOrUpdatedEventArgs(challengeInfo, challenge));
	    }

        protected void OnLocalRecordsDetermined(List<RankEntry> ranks)
        {
            if (LocalRecordsDetermined != null)
                LocalRecordsDetermined(this, new EventArgs<RankEntry[]>(ranks.ToArray()));
        }

	    #endregion
	}

    public class PlayerVoteEventArgs : EventArgs
    {
        #region Properties

        public string Login { get; private set; }
        public int ChallengeID { get; private set; }
        public ushort VoteValue { get; private set; }
        public double AverageVoteValue { get; private set; }

        #endregion

        #region Constructor

        public PlayerVoteEventArgs(string login, int challengeID, ushort voteValue, double averageVoteValue)
        {
            Login = login;
            ChallengeID = challengeID;
            VoteValue = voteValue;
            AverageVoteValue = averageVoteValue;
        }

        #endregion
    }

    public class PlayerNewRecordEventArgs: EventArgs
    {
        #region Properties

        public PlayerInfo PlayerInfo { get; private set; }
        public int TimeOrScore { get; private set; }
        public uint? OldPosition { get; private set; }
        public uint? NewPosition { get; private set; }

        #endregion

        #region Constructor

        public PlayerNewRecordEventArgs(PlayerInfo playerInfo, int timeOrScore, uint? oldPosition, uint? newPosition)
        {
            PlayerInfo = playerInfo;
            TimeOrScore = timeOrScore;
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }

        #endregion
    }

    public class PlayerWinEventArgs : EventArgs
    {
        #region Properties

        public PlayerRank RankingInfo { get; private set; }
        public uint Wins { get; private set; }

        #endregion

        #region Constructor

        public PlayerWinEventArgs(PlayerRank rankingInfo, uint wins)
        {
            RankingInfo = rankingInfo;
            Wins = wins;
        }

        #endregion
    }

    public class PlayerCreatedOrUpdatedEventArgs : EventArgs
    {
        #region Properties

        public Player Player { get; private set; }
        public PlayerInfo PlayerInfo { get; private set; }

        #endregion

        #region Constructor

        public PlayerCreatedOrUpdatedEventArgs(Player player, PlayerInfo playerInfo)
        {
            Player = player;
            PlayerInfo = playerInfo;
        }

        #endregion
    }

    public class ChallengeCreatedOrUpdatedEventArgs : EventArgs
    {
        #region Properties

        public ChallengeListSingleInfo ChallengeInfo { get; private set; }
        public Challenge Challenge { get; private set; }

        #endregion

        #region Constructor

        public ChallengeCreatedOrUpdatedEventArgs(ChallengeListSingleInfo challengeInfo, Challenge challenge)
        {
            ChallengeInfo = challengeInfo;
            Challenge = challenge;
        }

        #endregion
    }
}