using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Logging;
using TMSPS.Core.PluginSystem.Configuration;
using Version=System.Version;
using System.Linq;

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
	    public IRankingAdapter RankingAdapter { get; protected set; }
	    public int CurrentChallengeID { get; protected set; }
	    public LocalRecordsSettings Settings { get; protected set; }
	    protected List<ILocalRecordsPluginPlugin> Plugins {get; private set;}
	    public RankEntry[] LocalRecords { get; private set; }

	    #endregion

	    #region Events

	    public event EventHandler<PlayerNewRecordEventArgs> PlayerNewRecord;
	    public event EventHandler<PlayerWinEventArgs> PlayerWins;
	    public event EventHandler<PlayerCreatedOrUpdatedEventArgs> PlayerCreatedOrUpdated;
	    public event EventHandler<ChallengeCreatedOrUpdatedEventArgs> ChallengeCreatedOrUpdated;
	    public event EventHandler<EventArgs<RankEntry[]>> LocalRecordsDetermined;
	    public event EventHandler ChallengeChanged;

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


	        foreach (PlayerSettings playerSettings in Context.PlayerSettings.GetAllAsList())
	        {
	            if (!playerSettings.NickName.IsNullOrTimmedEmpty())
	                PlayerAdapter.CreateOrUpdate(new Player(playerSettings.Login, playerSettings.NickName));
	        }

	        ChallengeInfo currentChallengeInfo = GetCurrentChallengeInfoCached();
	        if (currentChallengeInfo == null)
	        {
	            Logger.ErrorToUI(string.Format("An error occured. {0} not started!", Name));
	            return;
	        }

	        EnsureChallengeExistsInStorage(currentChallengeInfo);
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

        protected override void Dispose(bool connectionLost)
        {
            RunCatchLog(() => Context.PlayerSettings.GetAllAsList().ForEach(p => PlayerAdapter.UpdateTimePlayed(p.Login)), "Error updatimg TimePlayed for all players while disposing the plugin.");
            DisposePlugins(connectionLost);

            // enforce connection close here later

            Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerDisconnect -= Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.PlayerFinish -= Callbacks_PlayerFinish;
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
        }

	    private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
	    {
	        RunCatchLog(()=>
            {
                if (CheckForDeleteCheaterCommand(e))
                    return;

                if (CheckForListLocalLoginsCommand(e))
                    return;

            }, "Error in Callbacks_PlayerChat Method.", true);
	    }

        private void Callbacks_PlayerFinish(object sender, PlayerFinishEventArgs e)
        {
            RunCatchLog(() => ThreadPool.QueueUserWorkItem(OnPlayerFinished, new object[] { CurrentChallengeID, e }), "Error in Callbacks_PlayerFinish Method.", true);
        }

        private void Callbacks_EndRace(object sender, EndRaceEventArgs e)
        {
            RunCatchLog(() =>
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

        private void Callbacks_PlayerDisconnect(object sender, PlayerDisconnectEventArgs e)
        {
            RunCatchLog(() => PlayerAdapter.UpdateTimePlayed(e.Login), "Error in Callbacks_PlayerDisconnect Method.", true);
        }

        private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
        {
            if (e.Handled)
                return;

            RunCatchLog(() =>
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
            RunCatchLog(() =>
            {
                EnsureChallengeExistsInStorage(e.ChallengeInfo);
                DetermineLocalRecords();
                OnLocalRecordsDetermined(new List<RankEntry>(LocalRecords));

                if (ChallengeChanged != null)
                    ChallengeChanged(this, EventArgs.Empty);
            }, "Error in Callbacks_BeginRace Method.", true);
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

	    private bool CheckForDeleteCheaterCommand(PlayerChatEventArgs args)
	    {
            if (args.IsServerMessage || args.Text.IsNullOrTimmedEmpty())
                return false;

	        ServerCommand command = ServerCommand.Parse(args.Text);

	        if (!command.Is(Command.DeleteCheater) || command.PartsWithoutMainCommand.Count == 0)
	            return false;

	        string login = command.PartsWithoutMainCommand[0];

            if (!LoginHasRight(args.Login, true, Command.DeleteCheater))
                return true;
            
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
 
	        return true;
	    }

        private bool CheckForListLocalLoginsCommand(PlayerChatEventArgs args)
        {
            if (args.IsServerMessage || args.Text.IsNullOrTimmedEmpty())
                return false;

            ServerCommand command = ServerCommand.Parse(args.Text);

            if (!command.Is(Command.GetLocalLogins))
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

        private void UpdateRankingForChallenge(object challengeID)
        {
            RankingAdapter.UpdateForChallenge(Convert.ToString(challengeID));
        }

	    private void EnsureChallengeExistsInStorage(ChallengeListSingleInfo challengeInfo)
	    {
	        Challenge challenge = new Challenge(challengeInfo.UId, challengeInfo.Name, challengeInfo.Author, challengeInfo.Environnement);
	        ChallengeAdapter.IncreaseRaces(challenge);
	        CurrentChallengeID = challenge.ID.Value;

	        OnChallengeCreatedOrUpdated(challengeInfo, challenge);
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