using System;
using System.Collections.Generic;
using System.Threading;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;
using Version=System.Version;
using System.Linq;

namespace TMSPS.Core.PluginSystem.Plugins
{
    public partial class TMSPSCorePlugin : TMSPSPlugin
    {
        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "Core Plugin"; } }
        public override string Description { get { return "This Plugin does all the basic stuff making the whole thing work."; } }
        public override string ShortName { get { return "Core"; } }
        public TMSPSCorePluginSettings Settings { get; private set; }
        private Timer DedimaniaBlackListSyncTimer { get; set; }

        #endregion

        #region Constructor

        public TMSPSCorePlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion

        #region Methods

        protected override void Init()
        {
            Settings = TMSPSCorePluginSettings.ReadFromFile(PluginSettingsFilePath);
            NicknameResolverFactory.CreateSingleInstance(Settings, Context);

            List<PlayerInfo> players =  GetPlayerList(); 

            foreach (PlayerInfo playerInfo in players)
            {
                NicknameResolverFactory.Instance.Set(playerInfo.Login, playerInfo.NickName);
            }

            GetCurrentChallengeInfo(); // cache the current challenge info
            GetServerOptions(); // cache currenr server options
            GetCurrentGameMode(); // cache current game mode

            if (Settings.EnableDedimaniaBlackListSync)
                DedimaniaBlackListSyncTimer = new Timer(SyncBlackListWithDedimania, null, TimeSpan.Zero, Settings.DedimaniaBlackListSyncInterval);

            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.BeginChallenge += Callbacks_BeginChallenge;
            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerInfoChanged += Callbacks_PlayerInfoChanged;
        }

        protected override void Dispose(bool connectionLost)
        {
            if (Settings.EnableDedimaniaBlackListSync)
                DedimaniaBlackListSyncTimer.Dispose();

            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerDisconnect -= Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.BeginChallenge -= Callbacks_BeginChallenge;
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerInfoChanged -= Callbacks_PlayerInfoChanged;
        }

        private void Callbacks_PlayerInfoChanged(object sender, PlayerInfoChangedEventArgs e)
        {
            RunCatchLog(() => GetPlayerSettings(e.PlayerInfo.Login, true).UpdateFromPlayerInfo(e.PlayerInfo), "Errror in Callbacks_PlayerInfoChanged", true);
        }

        private void SyncBlackListWithDedimania(object state)
        {
            RunCatchLog(() =>
            {
                GenericListResponse<LoginResponse> getBlackListResponse = Context.RPCClient.Methods.GetBlackList(10000, 0);   

                if (getBlackListResponse.Erroneous)
                {
                    Logger.ErrorToUI(string.Format("Error while calling GetBlackList: {0}({1})", getBlackListResponse.Fault.FaultMessage, getBlackListResponse.Fault.FaultCode));
                    return;
                }

                HashSet<string> localBlackListLogins = new HashSet<string>(getBlackListResponse.Value.ConvertAll(x => x.Login));
                HashSet<string> dedimaniaBlackListLogins = BlackListReader.GetBlackListedLogins(new Uri(Settings.DedimaniaBlackListUrl));

                Logger.Debug(string.Format("Found {0} login(s) in local blacklist and {1} login(s) in dedimania blacklist.", localBlackListLogins.Count, dedimaniaBlackListLogins.Count));
                dedimaniaBlackListLogins.ExceptWith(localBlackListLogins);
                Logger.Debug(string.Format("{0} login(s) in will be added to blacklist.", dedimaniaBlackListLogins.Count));

                foreach (string login in dedimaniaBlackListLogins)
                {
                    GenericResponse<bool> blackListResponse = Context.RPCClient.Methods.BlackList(login);

                    if (blackListResponse.Erroneous)
                    {
                        Logger.Error(string.Format("Error while calling BlackList for login {0}: {1}({2})", login, blackListResponse.Fault.FaultMessage, blackListResponse.Fault.FaultCode));
                        continue;
                    }

                    if (blackListResponse.Value)
                        Logger.Debug(string.Format("Added login {0} to blacklist.", login));
                    else
                        Logger.Debug(string.Format("Could not add login {0} to blacklist.", login));
                }

                if (dedimaniaBlackListLogins.Count > 0)
                {
                    Logger.InfoToUI(string.Format("Added {0} login(s) from dedimania blacklist to local blacklist.", dedimaniaBlackListLogins.Count));
                    GenericResponse<bool> saveBlackListResponse = Context.RPCClient.Methods.SaveBlackList("blacklist.txt");

                    if (saveBlackListResponse.Erroneous)
                    {
                        Logger.Error(string.Format("Error while calling SaveBlackList: {0}({1})", saveBlackListResponse.Fault.FaultMessage, saveBlackListResponse.Fault.FaultCode));
                        return;
                    }

                    if (!saveBlackListResponse.Value)
                        Logger.Error("Could not save blacklist.");
                }
            }, "Errror in SyncBlackListWithDedimania", true);
        }

        private void Callbacks_EndRace(object sender, EndRaceEventArgs e)
        {
            RunCatchLog(() =>
            {          
                int top = Convert.ToInt32(Settings.SaveGhostReplayOfTop);

                if (top < 100)
                {
                    foreach (PlayerRank rank in e.Rankings.Take(top).Where(x => x.BestTime > 0))
                    {
                        Context.RPCClient.Methods.SaveBestGhostsReplay(rank.Login, string.Empty);
                    }
                }
                else
                    Context.RPCClient.Methods.SaveBestGhostsReplay(string.Empty, string.Empty);
            }, "Errror in Callbacks_EndRace", true);
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            RunCatchLog(() =>
            { 
                ServerCommand command = ServerCommand.Parse(e.Text);

                if (command != null)
                    HandleCommand(e.Login, command);
            }, "Errror in Callbacks_PlayerChat", true);
        }

        private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
        {
            RunCatchLog(() =>
            {
                PlayerInfo playerInfo = GetPlayerInfo(e.Login);

                if (playerInfo == null)
                {
                    e.Handled = true;
                    Context.RPCClient.Methods.Kick(e.Login, "TMSPS couldn't determine your player information, try reconnecting!");
                    return;
                }

                NicknameResolverFactory.Instance.Set(e.Login, playerInfo.NickName);

                if (playerInfo.NickName.IsNullOrTimmedEmpty())
                {
                    Context.RPCClient.Methods.Kick(e.Login, "Please provide a nickname!");
                    e.Handled = true;
                    return;
                }

                if (Settings.EnableJoinMessage)
                {
                    DetailedPlayerInfo detailedPlayerInfo = GetDetailedPlayerInfo(e.Login);

                    if (detailedPlayerInfo == null)
                        return;

                    string nation = "Unknown";
                    List<string> pathParts = new List<string>(detailedPlayerInfo.Path.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

                    if (pathParts.Count > 1)
                        nation = string.Join(" > ", pathParts.ToArray(), 1, pathParts.Count - 1);

                    int ladderRank = -1;

                    PlayerRanking worldRanking = detailedPlayerInfo.LadderStats.PlayerRankings.Find(ranking => ranking.Path == "World");

                    if (worldRanking != null)
                        ladderRank = worldRanking.Ranking;

                    SendFormattedMessage(Settings.JoinMessage, "Nickname", StripTMColorsAndFormatting(detailedPlayerInfo.NickName), "Nation", nation, "Ladder", ladderRank.ToString(Context.Culture));
                }
            }, "Error in Callbacks_PlayerConnect Method.", true);
        }

        private void Callbacks_PlayerDisconnect(object sender, PlayerDisconnectEventArgs e)
        {
            RunCatchLog(() =>
            {
                Context.PlayerSettings.Remove(e.Login.ToLower());

                if (Settings.EnableLeaveMessage)
                {
                    string nickname = GetNickname(e.Login, true);

                    if (nickname != null)
                        SendFormattedMessage(Settings.LeaveMessage, "Nickname", StripTMColorsAndFormatting(nickname));
                }
            }, "Error in Callbacks_PlayerDisconnect Method.", true);
        }

        private void Callbacks_BeginChallenge(object sender, BeginChallengeEventArgs e)
        {
            RunCatchLog(() =>
            {
                CurrentChallengeInfo = e.ChallengeInfo; // cache current challenge info
                GetServerOptions(); // cache current server options
                GetCurrentGameMode(); // cache current game mode
            }, "Error in Callbacks_BeginChallenge Method.", true);
        }

        

        #endregion
    }
}