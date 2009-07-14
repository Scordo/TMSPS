using System;
using System.Text;
using System.Threading;
using System.Windows;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using Timer=System.Windows.Forms.Timer;
using Version=TMSPS.Core.Communication.ProxyTypes.Version;

namespace TMSPS.TRC.Controls
{
    /// <summary>
    /// Interaction logic for ServerInfoTabContentControl.xaml
    /// </summary>
    public partial class ServerInfoTabContentControl
    {
        private bool _loaded = false;

        private int PlayersCount { get; set; }
        private int MaxPlayersCount { get; set; }
        private Timer NetworkStatsTimer { get; set; }

        public ServerInfoTabContentControl()
        {
            InitializeComponent();
            NetworkStatsTimer = new Timer();
            NetworkStatsTimer.Tick += NetworkStatsTimer_Tick;
            NetworkStatsTimer.Interval = 60 * 1000;
        }

        private void NetworkStatsTimer_Tick(object sender, EventArgs e)
        {
            FillNetworkStats();
        }

        private void ServerInfoTabContentControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loaded)
                return;
            
            _loaded = true;
            RPCClient.Callbacks.StatusChanged += Callbacks_StatusChanged;
            RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
            RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
        }

        public override void DoWork()
        {
            base.DoWork();

            NetworkStatsTimer.Enabled = true;
            Reload();
        }

        public override void StopWork()
        {
            base.StopWork();
            NetworkStatsTimer.Enabled = false;
        }

        public override void Reload()
        {
            ThreadPool.QueueUserWorkItem(ReloadInternal, null);
        }

        private void ReloadInternal(object state)
        {
            Action action = () =>
            {
                FillServerStatusTextBlock();
                FillServerNameTextBlock();
                FillServerVersionTextBlock();
                FillServerIPTextBlock();
                FillPlayersTextBlock();
                FillLadderLimitTextBlock();
                FillCurrentChallengeTextBlock();
                FillIsInternetServerCheckBox();
                FillNetworkStats();
            };

            Dispatcher.Invoke(action, null);
        }

        private void FillServerStatusTextBlock()
        {
            GenericResponse<ServerStatus> statusResponse = RPCClient.Methods.GetStatus();

            if (statusResponse.Erroneous)
                return; // log here later

            ServerStatusTextBlock.Text = statusResponse.Value.Name;
        }

        private void FillServerVersionTextBlock()
        {
            GenericResponse<Version> versionResponse = RPCClient.Methods.GetVersion();

            if (versionResponse.Erroneous)
                return; // log here later

            ServerVersionTextBlock.Text = string.Format("{0} - {1} ({2})", versionResponse.Value.Name, versionResponse.Value.VersionString, versionResponse.Value.Build);
        }

        private void FillServerNameTextBlock()
        {
            GenericResponse<string> serverNameResponse = RPCClient.Methods.GetServerName();

            if (serverNameResponse.Erroneous)
                return; // log here later

            ServerNameTextBlock.Text = serverNameResponse.Value;
        }

        private void FillPlayersTextBlock()
        {
            GenericResponse<CNPair<int>> maxPlayersResponse = RPCClient.Methods.GetMaxPlayers();

            if (maxPlayersResponse.Erroneous)
                return; // log here later

            GenericListResponse<PlayerInfo> playerListResponse = RPCClient.Methods.GetPlayerList();

            if (playerListResponse.Erroneous)
                return; // log here later

            PlayersCount = playerListResponse.Value.Count;
            MaxPlayersCount = maxPlayersResponse.Value.CurrentValue;

            FillPlayersTextBlock(PlayersCount, MaxPlayersCount);
        }

        private void FillPlayersTextBlock(int players, int maxPlayers)
        {
            PlayersTextBlock.Text = string.Format("{0}/{1}", players, maxPlayers);
        }

        private void FillServerIPTextBlock()
        {
            GenericResponse<SystemInfo> systemInfoResponse = RPCClient.Methods.GetSystemInfo();

            if (systemInfoResponse.Erroneous)
                return; // log here later

            ServerIPTextBlock.Text = string.Format("{0}:{1}", systemInfoResponse.Value.PublishedIp, systemInfoResponse.Value.Port);
        }

        private void FillLadderLimitTextBlock()
        {
            GenericResponse<LadderServerLimits> ladderLimitResponse = RPCClient.Methods.GetLadderServerLimits();

            if (ladderLimitResponse.Erroneous)
                return; // log here later

            LadderLimitTextBlock.Text = string.Format("{0}/{1}", Convert.ToInt32(Math.Floor(ladderLimitResponse.Value.LadderServerLimitMin)), Convert.ToInt32(Math.Floor(ladderLimitResponse.Value.LadderServerLimitMax)));
        }

        private void FillCurrentChallengeTextBlock()
        {
            GenericResponse<ChallengeInfo> currentChallengeInfoResponse = RPCClient.Methods.GetCurrentChallengeInfo();

            if (currentChallengeInfoResponse.Erroneous)
                return; // log here later

            CurrentChallengeTextBlock.Text = currentChallengeInfoResponse.Value.Name;
        }

        private void FillIsInternetServerCheckBox()
        {
            //GenericResponse<ChallengeInfo> currentChallengeInfoResponse = RPCClient.Methods.GetSystemInfo();

            //if (currentChallengeInfoResponse == null)
            //    return; // log here later

            IsInternetServerCheckBox.IsChecked = true;
        }

        private void FillNetworkStats()
        {
            GenericResponse<NetworkStatus> networkStatusResponse = RPCClient.Methods.GetNetworkStats();

            if (networkStatusResponse.Erroneous)
                return; // log here later

            NetworkStatus networkStatus = networkStatusResponse.Value;
            UptimeTextBlock.Text = SecondsToTimeString(networkStatus.Uptime);
            AvgConnectionTimeTextBlock.Text = SecondsToTimeString(networkStatus.MeanConnectionTime);
            AvgPlayersTextBlock.Text = networkStatus.MeanNbrPlayer.ToString();
            ConnectionsTextBlock.Text = networkStatus.NbrConnection.ToString();
            RecvNetRateTextBlock.Text = networkStatus.RecvNetRate.ToString();
            SendNetRateTextBlock.Text = networkStatus.SendNetRate.ToString();
            TotalReceivingSizeTextBlock.Text = CreateFileSizeString(networkStatus.TotalReceivingSize);
            TotalSendingSizeTextBlock.Text = CreateFileSizeString(networkStatus.TotalSendingSize);
        }

        private static string SecondsToTimeString(int seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);

            StringBuilder result = new StringBuilder();

            int weeks = timeSpan.Days / 7;
            int days = timeSpan.Days % 7;

            if (weeks > 0)
                result.AppendFormat("{0}w", weeks);

            if (days > 0)
                result.AppendFormat(" {0}d", days);

            if (timeSpan.Hours > 0)
                result.AppendFormat(" {0}h", timeSpan.Hours);

            if (timeSpan.Minutes > 0)
                result.AppendFormat(" {0}m", timeSpan.Minutes);

            if (timeSpan.Seconds > 0)
                result.AppendFormat(" {0}s", timeSpan.Seconds);

            return result.ToString().TrimStart();
        }

        private static string CreateFileSizeString(long sizeInBytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };

            int run = 0;
            double doubleSize = Convert.ToDouble(sizeInBytes);
            
            while (doubleSize >= 1024)
            {
                doubleSize /= 1024;
                run++;
            }

            return string.Format("{0} {1}", Math.Round(doubleSize, 2, MidpointRounding.AwayFromZero), sizes[run]);
        }


        protected override void EnableControls()
        {

        }

        protected override void DisableControls()
        {
             
        }

        private void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            RPCClient.Connect();
        }

        private void Callbacks_StatusChanged(object sender, Core.Communication.EventArguments.Callbacks.StatusChangedEventArgs e)
        {
            Action action = () => ServerStatusTextBlock.Text = e.StatusName;
            Dispatcher.Invoke(action, null);
        }

        private void Callbacks_BeginRace(object sender, Core.Communication.EventArguments.Callbacks.BeginRaceEventArgs e)
        {
            Action action = FillPlayersTextBlock;
            Dispatcher.Invoke(action, null);

            action = FillCurrentChallengeTextBlock;
            Dispatcher.Invoke(action, null);
        }

        private void Callbacks_PlayerConnect(object sender, Core.Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            PlayersCount++;

            Action action = () => FillPlayersTextBlock(PlayersCount, MaxPlayersCount);
            Dispatcher.Invoke(action, null); 
        }

        private void Callbacks_PlayerDisconnect(object sender, Core.Communication.EventArguments.Callbacks.PlayerDisconnectEventArgs e)
        {
            PlayersCount--;

            Action action = () => FillPlayersTextBlock(PlayersCount, MaxPlayersCount);
            Dispatcher.Invoke(action, null); 
        }
    }
}