﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Logging;
using Version=System.Version;
using System.Linq;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;
using Timer=System.Timers.Timer;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class LocalRecordsPlugin : TMSPSPlugin
	{
	    #region Constants

	    private const string DELETE_CHEATER_COMMAND = "DeleteCheater";
        private const string GET_LOCAL_LOGINS_COMMAND = "GetLocalLogins";

	    #endregion

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
	    public IRankingAdapter RankingAdapter { get; protected set; }
	    public int CurrentChallengeID { get; protected set; }
	    protected Timer TimePlayedTimer { get; private set; }
	    public LocalRecordsSettings Settings { get; protected set; }
	    protected List<ILocalRecordsPluginPlugin> Plugins {get; private set;}
	    public RankEntry[] LocalRecords { get; private set; }

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
	            RankingAdapter = AdapterProvider.GetRankingAdapter();
	        }
	        catch (Exception ex)
	        {
	            Logger.Error("Error initializing AdapterProvider for local records.", ex);
	            Logger.ErrorToUI(string.Format("An error occured. {0} not started!", Name));
	            return;
	        }

	        List<ChallengeListSingleInfo> challenges = GetChallengeList();
	        if (challenges == null)
	            return;

	    	try
	    	{
				Logger.InfoToUI("Starting to delete data of missing tracks");
				int amountOfDeletedTracks = ChallengeAdapter.DeleteTracksNotInProvidedList(challenges.ConvertAll(c => c.UId));
				Logger.InfoToUI(string.Format("Data of {0} Track(s) has been deleted.", amountOfDeletedTracks));
	    	}
	    	catch (Exception ex)
	    	{
	    		Logger.ErrorToUI("Couldn't delete data of missing tracks.", ex);
	    	}

	        List<PlayerInfo> players = GetPlayerList();
	        if (players == null)
	            return;

	        foreach (PlayerInfo playerInfo in players)
	        {
	            if (!playerInfo.NickName.IsNullOrTimmedEmpty())
	                PlayerAdapter.CreateOrUpdate(new Player(playerInfo.Login, playerInfo.NickName));
	        }

	        ChallengeInfo currentChallengeInfo = GetCurrentChallengeInfoCached();
	        if (currentChallengeInfo == null)
	        {
	            Logger.ErrorToUI(string.Format("An error occured. {0} not started!", Name));
	            return;
	        }

	        EnsureChallengeExistsInStorage(currentChallengeInfo);
	        
	        TimePlayedTimer = new Timer(30000);
	        TimePlayedTimer.Elapsed += TimePlayedTimer_Elapsed;
	        TimePlayedTimer.Start();

	        DetermineLocalRecords();

	        InitializePlugins();
	        OnLocalRecordsDetermined(new List<RankEntry>(LocalRecords));

            Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
	        Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
	        Context.RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
	        Context.RPCClient.Callbacks.PlayerFinish += Callbacks_PlayerFinish;
	        Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
	    }

        private void HandleCheater(string login, bool updateUI)
        {
            Context.RPCClient.Methods.BanAndBlackList(login, "Banned and blacklisted for cheating!", true);
            SendFormattedMessage(Settings.CheaterBannedMessage, "Login", login);
            PlayerAdapter.RemoveAllStatsForLogin(login);

            if (updateUI)
            {
                DetermineLocalRecords();
                OnLocalRecordsDetermined(new List<RankEntry>(LocalRecords));
            }
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
                if (CheckForVotingCommand(e))
                    return;

                if (CheckForDeleteCheaterCommand(e))
                    return;

                if (CheckForListLocalLoginsCommand(e))
                    return;

            }, "Error in Callbacks_PlayerChat Method.", true);
	    }

	    private bool CheckForVotingCommand(PlayerChatEventArgs args)
	    {
	        if (args.IsServerMessage || args.Text.IsNullOrTimmedEmpty() || args.IsRegisteredCommand)
	            return false;

	        string message = args.Text.Trim();
	        ushort? voteValue;

	        Dictionary<string, ushort?> voteValues = new Dictionary<string, ushort?> {{ "++", 8 }, { "--", 0 }, { "+-", 4 }, { "-+", 4 }, 
	                                                                                  { "+1", 1 }, { "+2", 2 }, { "+3", 3 }, { "+4", 4 }, 
	                                                                                  { "+5", 5 }, { "+6", 6 }, { "+7", 7 }, { "+8", 8 }};

	        voteValues.TryGetValue(message, out voteValue);

	        if (voteValue.HasValue)
	        {
	            double? averageVote = RatingAdapter.Vote(args.Login, CurrentChallengeID, voteValue.Value);

	            if (averageVote.HasValue)
	                OnPlayerVoted(args.Login, CurrentChallengeID, voteValue.Value, averageVote.Value);
	        }

	        return true;
	    }

	    private bool CheckForDeleteCheaterCommand(PlayerChatEventArgs args)
	    {
            if (args.IsServerMessage || args.Text.IsNullOrTimmedEmpty())
                return false;

	        ServerCommand command = ServerCommand.Parse(args.Text);

	        if (command == null || !command.IsMainCommandAnyOf(DELETE_CHEATER_COMMAND) || command.PartsWithoutMainCommand.Count < 1)
	            return false;

	        string login = command.PartsWithoutMainCommand[0];

            if (Context.Credentials.UserHasRight(args.Login, DELETE_CHEATER_COMMAND))
            {
                if (PlayerAdapter.RemoveAllStatsForLogin(login))
                {
                    SendFormattedMessageToLogin(args.Login, Settings.CheaterDeletedMessage, "Login", login);

                    DetermineLocalRecords();
                    OnLocalRecordsDetermined(new List<RankEntry>(LocalRecords));
                }
                else
                {
                    SendFormattedMessageToLogin(args.Login, Settings.CheaterDeletionFailedMessage, "Login", login);
                }
            }
            else
            {
                SendNoPermissionMessagetoLogin(args.Login);
            }

	        return true;
	    }

        private bool CheckForListLocalLoginsCommand(PlayerChatEventArgs args)
        {
            if (args.IsServerMessage || args.Text.IsNullOrTimmedEmpty())
                return false;

            ServerCommand command = ServerCommand.Parse(args.Text);

            if (command == null || !command.IsMainCommandAnyOf(GET_LOCAL_LOGINS_COMMAND))
                return false;

            DetermineLocalRecords();

            StringBuilder msg = new StringBuilder("Local logins: ");

            for (int i = 0; i < LocalRecords.Length; i++)
            {
                if (i != 0)
                    msg.Append(", ");
                RankEntry entry = LocalRecords[i];
                msg.AppendFormat("{0}. {1}$z[{2}]", i + 1, entry.Nickname, entry.Login);
            }

            SendFormattedMessageToLogin(args.Login, msg.ToString());
            return true;
        }

	    private void Callbacks_PlayerFinish(object sender, PlayerFinishEventArgs e)
	    {
	        if (e.Erroneous)
	        {
	            Logger.Error(string.Format("[Callbacks_PlayerFinish] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
	            return;
	        }

            RunCatchLog(() => ThreadPool.QueueUserWorkItem(OnPlayerFinished, new object[] { CurrentChallengeID, e}), "Error in Callbacks_PlayerFinish Method.", true);
	    }

	    private void OnPlayerFinished(object state)
	    {
	        object[] stateParams = (object[]) state;
	        int currentChallengeID = (int) stateParams[0];
            PlayerFinishEventArgs e = (PlayerFinishEventArgs)stateParams[1];

	        if (e.TimeOrScore > 0)
	        {
	            uint? oldPosition, newPosition;
	            bool newBest;
                RecordAdapter.CheckAndWriteNewRecord(e.Login, currentChallengeID, e.TimeOrScore, out oldPosition, out newPosition, out newBest);

	            if (newBest)
	            {
                    string nickname = GetNickname(e.Login);

                    if (nickname != null && newPosition <= Settings.MaxRecordsToReport && currentChallengeID == CurrentChallengeID)
	                {
                        DetermineLocalRecords();
                        OnPlayerNewRecord(e.Login, nickname, e.TimeOrScore, oldPosition, newPosition);
	                }
	            }

                SessionAdapter.AddSession(e.Login, currentChallengeID, Convert.ToUInt32(e.TimeOrScore));
	        }
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
                if (e.Rankings.Count > 0)
                {
                    // this may take a while, so run it async on the thread pool
                    ThreadPool.QueueUserWorkItem(UpdateRankingForChallenge, e.Challenge.UId);
                }

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
                            if (playerRank.Rank <= 0)
                                continue;

                            if (!CheckpointsValid(playerRank.BestCheckpoints))
                                HandleCheater(playerRank.Login, false);
                            else
                                PositionAdapter.AddPosition(playerRank.Login, e.Challenge.UId, Convert.ToUInt16(playerRank.Rank), Convert.ToUInt16(maxRank));
                        }
                    }
                }
            }, "Error in Callbacks_EndRace Method.", true);
        }

        private void UpdateRankingForChallenge(object challengeID)
        {
            RankingAdapter.UpdateForChallenge(Convert.ToString(challengeID));
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

	        if (e.Handled)
	            return;

	        RunCatchLog(()=>
            {
                string nickname = GetNickname(e.Login);

                if (nickname == null)
                    return;

                if (!nickname.IsNullOrTimmedEmpty())
                {
                    Player player = new Player(e.Login, nickname);
                    PlayerAdapter.CreateOrUpdate(player);
                    OnPlayerCreatedOrUpdated(player, nickname);
                }
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
                OnLocalRecordsDetermined(new List<RankEntry>(LocalRecords));
            }, "Error in Callbacks_BeginRace Method.", true);
	    }

	    private void EnsureChallengeExistsInStorage(ChallengeListSingleInfo challengeInfo)
	    {
	        Challenge challenge = new Challenge(challengeInfo.UId, challengeInfo.Name, challengeInfo.Author, challengeInfo.Environnement);
	        ChallengeAdapter.IncreaseRaces(challenge);
	        CurrentChallengeID = challenge.ID.Value;

	        OnChallengeCreatedOrUpdated(challengeInfo, challenge);
	    }

	    private void UpdateTimePlayedForAllCurrentPlayers()
	    {
	        List<PlayerInfo> players = GetPlayerList();

	        if (players == null)
	            return;

            PlayerAdapter.UpdateTimePlayed(players.ConvertAll(p => p.Login));  
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
	        LocalRecords = RecordAdapter.GetTopRecordsForChallenge(CurrentChallengeID, Settings.MaxRecordsToReport).ToArray();
	        Context.ValueStore.SetOrUpdate(GlobalConstants.LOCAL_RECORDS, LocalRecords.ToArray());
	        Context.ValueStore.SetOrUpdate(GlobalConstants.FIRST_LOCAL_RECORD_TIMEORSCORE, LocalRecords.Length == 0 ? null : (int?)LocalRecords[0].TimeOrScore);
	    }

	    private void DisposePlugins(bool connectionLost)
	    {
	        foreach (ILocalRecordsPluginPlugin plugin in Plugins)
	        {
	            plugin.DisposePlugin(connectionLost);
	        }
	    }

	    protected override void Dispose(bool connectionLost)
	    {
	        TimePlayedTimer.Stop();
            
	        if (!connectionLost)
	            UpdateTimePlayedForAllCurrentPlayers();

	        DisposePlugins(connectionLost);

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

	    protected void OnPlayerNewRecord(string login, string nickname, int timeOrScore, uint? oldPosition, uint? newPosition)
	    {
	        if (PlayerNewRecord != null)
                PlayerNewRecord(this, new PlayerNewRecordEventArgs(login, nickname, timeOrScore, oldPosition, newPosition));
	    }

	    protected void OnPlayerWins(PlayerRank rankingInfo, uint wins)
	    {
	        if (PlayerWins != null)
	            PlayerWins(this, new PlayerWinEventArgs(rankingInfo, wins));
	    }

        protected void OnPlayerCreatedOrUpdated(Player player, string nickname)
	    {
	        if (PlayerCreatedOrUpdated != null)
                PlayerCreatedOrUpdated(this, new PlayerCreatedOrUpdatedEventArgs(player, nickname));
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

        public string Nickname { get; private set; }
        public string Login { get; private set; }
        public int TimeOrScore { get; private set; }
        public uint? OldPosition { get; private set; }
        public uint? NewPosition { get; private set; }

        #endregion

        #region Constructor

        public PlayerNewRecordEventArgs(string login, string nickname, int timeOrScore, uint? oldPosition, uint? newPosition)
        {
            Login = login;
            Nickname = nickname;
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
        public string Nickname { get; private set; }

        #endregion

        #region Constructor

        public PlayerCreatedOrUpdatedEventArgs(Player player, string nickname)
        {
            Player = player;
            Nickname = nickname;
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