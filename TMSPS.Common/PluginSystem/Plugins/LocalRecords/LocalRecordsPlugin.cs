using System;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class LocalRecordsPlugin : TMSPSPluginBase
	{
		public override Version Version
		{
			get { return new Version("1.0.0.0"); }
		}

		public override string Author
		{
			get { return "Jens Hofmann"; }
		}

		public override string Name
		{
			get { return "Local Records Plugin"; }
		}

		public override string Description
		{
			get { return "Saves records and statistics in a local database."; }
		}

		public override string ShortNameForLogging
		{
			get { return "LocalRecords"; }
		}

		private IAdapterProvider AdapterProvider { get; set; }
		private IChallengeAdapter ChallengeAdapter { get; set;}
        private IPlayerAdapter PlayerAdapter { get; set; }
		private int CurrentChallengeID { get; set; }
        private System.Timers.Timer TimePlayedTimer { get; set; }

		protected override void Init()
		{
			try
			{
				AdapterProvider = AdapterProviderFactory.GetAdapterProvider(Util.GetCalculatedPath("LocalRecords.xml"));
				ChallengeAdapter = AdapterProvider.GetChallengeAdapter();
			    PlayerAdapter = AdapterProvider.GetPlayerAdapter();
			}
			catch (Exception ex)
			{
				Logger.Error("Error initializing AdapterProvider for local records.", ex);
				Logger.ErrorToUI(string.Format("An error occured. {0} not started!", Name));
				return;
			}

		    GenericListResponse<PlayerInfo> playersResponse = Context.RPCClient.Methods.GetPlayerList();

            if (playersResponse.Erroneous)
            {
                Logger.Error("Error getting PlayerList: "+playersResponse.Fault.FaultMessage);
                Logger.ErrorToUI("An error occured during player list retrieval!");
                return;
            }

		    foreach (PlayerInfo playerInfo in playersResponse.Value)
		    {
		        PlayerAdapter.CreateOrUpdate(new Player(playerInfo.Login, playerInfo.NickName));
		    }


            TimePlayedTimer = new System.Timers.Timer(30000);
            TimePlayedTimer.Elapsed += TimePlayedTimer_Elapsed;
            TimePlayedTimer.Start();
			
			Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
		}

        private void Callbacks_EndRace(object sender, Communication.EventArguments.Callbacks.EndRaceEventArgs e)
        {
            if (e.Rankings.Count > 1)
            {
                // there must be at least 2 players to increase the wins for the first player
                if (e.Rankings[0].BestTime > 0)
                    PlayerAdapter.IncreaseWins(e.Rankings[0].Login);
            }
        }

        private void TimePlayedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateTimePlayedForAllCurrentPlayers();
        }

        private void Callbacks_PlayerDisconnect(object sender,Communication.EventArguments.Callbacks.PlayerDisconnectEventArgs e)
        {
            PlayerAdapter.UpdateTimePlayed(e.Login);
        }

        private void Callbacks_PlayerConnect(object sender, Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            GenericResponse<PlayerInfo> playerInfoResponse = Context.RPCClient.Methods.GetPlayerInfo(e.Login);

            if (playerInfoResponse.Erroneous)
            {
                Logger.Error(string.Format("Error getting Playerinfo for player with login {0}: {1}", e.Login, playerInfoResponse.Fault.FaultMessage));
                Logger.ErrorToUI(string.Format("Error getting Playerinfo for player with login {0}", e.Login));
                return;
            }

            PlayerAdapter.CreateOrUpdate(new Player(playerInfoResponse.Value.Login, playerInfoResponse.Value.NickName));
        }

		private void Callbacks_BeginRace(object sender, Communication.EventArguments.Callbacks.BeginRaceEventArgs e)
		{
			Challenge challenge = new Challenge(e.ChallengeInfo.UId, e.ChallengeInfo.Name, e.ChallengeInfo.Author, e.ChallengeInfo.Environnement);
			ChallengeAdapter.IncreaseRaces(challenge);
			CurrentChallengeID = challenge.ID.Value;
		}

        private void UpdateTimePlayedForAllCurrentPlayers()
        {
            GenericListResponse<PlayerInfo> playersResponse = Context.RPCClient.Methods.GetPlayerList();

            if (playersResponse.Erroneous)
            {
                Logger.Error("Error getting PlayerList: " + playersResponse.Fault.FaultMessage);
                Logger.ErrorToUI("An error occured during player list retrieval!");
                return;
            }

            foreach (PlayerInfo playerInfo in playersResponse.Value)
            {
                PlayerAdapter.UpdateTimePlayed(playerInfo.Login);
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
		}
	}
}
