using System;
using System.Collections.Generic;
using System.Timers;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using Version=System.Version;
using System.Linq;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class LocalRecordsPlugin : TMSPSPluginBase
	{
	    #region Properties

	    public override Version Version { get { return new Version("1.0.0.0"); } }
	    public override string Author { get { return "Jens Hofmann"; } }
	    public override string Name{ get { return "Local Records Plugin"; } }
	    public override string Description { get { return "Saves records and statistics in a local database."; } }
	    public override string ShortNameForLogging { get { return "LocalRecords"; } }

	    private IAdapterProvider AdapterProvider { get; set; }
	    private IChallengeAdapter ChallengeAdapter { get; set;}
	    private IPlayerAdapter PlayerAdapter { get; set; }
	    private IPositionAdapter PositionAdapter { get; set; }
	    private IRecordAdapter RecordAdapter { get; set; }
	    private int CurrentChallengeID { get; set; }
	    private Timer TimePlayedTimer { get; set; }
	    private Dictionary<string, PlayerInfo> PlayerInfoCache { get; set; }

	    #endregion

	    #region Methods

	    protected override void Init()
	    {
	        PlayerInfoCache = new Dictionary<string, PlayerInfo>();
	        try
	        {
	            AdapterProvider = AdapterProviderFactory.GetAdapterProvider(Util.GetCalculatedPath("LocalRecords.xml"));
	            ChallengeAdapter = AdapterProvider.GetChallengeAdapter();
	            PlayerAdapter = AdapterProvider.GetPlayerAdapter();
	            PositionAdapter = AdapterProvider.GetPositionAdapter();
	            RecordAdapter = AdapterProvider.GetRecordAdapter();
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
			
	        Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
	        Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
	        Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
	        Context.RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
	        Context.RPCClient.Callbacks.PlayerFinish += Callbacks_PlayerFinish;
	    }

	    private void Callbacks_PlayerFinish(object sender, PlayerFinishEventArgs e)
	    {
	        if (e.TimeOrScore > 0)
	        {
	            uint? oldPosition, newPosition;
	            bool newBest;
	            RecordAdapter.CheckAndWriteNewRecord(e.Login, CurrentChallengeID, e.TimeOrScore, out oldPosition, out newPosition, out newBest);

	            if (newBest)
	            {
	                PlayerInfo playerInfo = GetPlayerInfo(e.Login, true);

	                if (playerInfo != null)
	                {
	                    if (oldPosition == null)
	                        Context.RPCClient.Methods.SendNotice(string.Format("{0}$z got his first local rank: $w$s$0f0{1}$z!", playerInfo.NickName, newPosition));
	                    else if (newPosition > oldPosition)
	                        Context.RPCClient.Methods.SendNotice(string.Format("{0}$z achieved local rank: $w$s$0f0{1}$z. Old rank: $w$s{2}", playerInfo.NickName, newPosition, oldPosition));
	                    else
	                        Context.RPCClient.Methods.SendNotice(string.Format("{0}$z improved his/her local rank: $w$s$0f0{1}$z!", playerInfo.NickName, newPosition));
	                }
	            }
	        }
	    }

	    private void Callbacks_EndRace(object sender, EndRaceEventArgs e)
	    {
	        if (e.Rankings.Count > 1)
	        {
	            // there must be at least 2 players to increase the wins for the first player
	            if (e.Rankings[0].BestTime > 0)
	            {
	                PlayerAdapter.IncreaseWins(e.Rankings[0].Login);

	                int maxRank = e.Rankings.Max(playerRank => playerRank.Rank);

	                foreach (PlayerRank playerRank in e.Rankings)
	                {
	                    PositionAdapter.AddPosition(playerRank.Login, e.Challenge.UId, Convert.ToUInt16(playerRank.Rank), Convert.ToUInt16(maxRank));
	                }
	            }
	        }
	    }

	    private void TimePlayedTimer_Elapsed(object sender, ElapsedEventArgs e)
	    {
	        UpdateTimePlayedForAllCurrentPlayers();
	    }

	    private void Callbacks_PlayerDisconnect(object sender,PlayerDisconnectEventArgs e)
	    {
	        PlayerAdapter.UpdateTimePlayed(e.Login);
	    }

	    private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
	    {
	        PlayerInfo playerInfo = GetPlayerInfo(e.Login);

	        if (playerInfo == null)
	            return;

	        PlayerAdapter.CreateOrUpdate(new Player(playerInfo.Login, playerInfo.NickName));
	    }

	    private void Callbacks_BeginRace(object sender, BeginRaceEventArgs e)
	    {
	        EnsureChallengeExistsInStorage(e.ChallengeInfo);
	    }

	    private void EnsureChallengeExistsInStorage(ChallengeListSingleInfo challengeInfo)
	    {
	        Challenge challenge = new Challenge(challengeInfo.UId, challengeInfo.Name, challengeInfo.Author, challengeInfo.Environnement);
	        ChallengeAdapter.IncreaseRaces(challenge);
	        CurrentChallengeID = challenge.ID.Value;
	    }

	    private PlayerInfo GetPlayerInfo(string login, bool allowCached)
	    {
	        if (!allowCached || !PlayerInfoCache.ContainsKey(login))
	            return GetPlayerInfo(login);

	        return PlayerInfoCache[login];
	    }

	    private PlayerInfo GetPlayerInfo(string login)
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

	    private List<PlayerInfo> GetPlayerList()
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

	    protected override void Dispose()
	    {
	        TimePlayedTimer.Stop();
	        UpdateTimePlayedForAllCurrentPlayers();

	        Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
	        Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
	        Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
	        Context.RPCClient.Callbacks.PlayerDisconnect -= Callbacks_PlayerDisconnect;
	        Context.RPCClient.Callbacks.PlayerFinish -= Callbacks_PlayerFinish;
	    }

	    #endregion
	}
}