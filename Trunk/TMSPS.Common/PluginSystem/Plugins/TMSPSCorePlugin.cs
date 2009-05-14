using System;
using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;
using Version=System.Version;
using System.Linq;

namespace TMSPS.Core.PluginSystem.Plugins
{
    internal partial class TMSPSCorePlugin : TMSPSPlugin
    {
        #region Properties

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
            get { return "Core Plugin"; }
        }

        public override string Description
        {
            get { return "This Plugin does all the basic stuff making the whole thing work."; }
        }

        public override string ShortName
        {
            get { return "Core"; }
        }

        private TMSPSCorePluginSettings Settings
        {
            get; set;
        }

        #endregion

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

            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.BeginChallenge += Callbacks_BeginChallenge;
            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
        }

        private void Callbacks_EndRace(object sender, EndRaceEventArgs e)
        {
            int top = Convert.ToInt32(Settings.SaveGhostReplayOfTop);

            if (top < 100)
            {
                foreach (PlayerRank rank in e.Rankings.Take(top))
                {
                    Context.RPCClient.Methods.SaveBestGhostsReplay(rank.Login, string.Empty);
                }
            }
            else
                Context.RPCClient.Methods.SaveBestGhostsReplay(string.Empty, string.Empty);
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            ServerCommand command = ServerCommand.Parse(e.Text);

            if (command != null)
                HandleCommand(e.Login, command);
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerDisconnect -= Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.BeginChallenge -= Callbacks_BeginChallenge;
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
        }

        private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
        {
            if (e.Erroneous)
            {
                Logger.Error(string.Format("[Callbacks_PlayerConnect] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
                return;
            }

            RunCatchLog(() =>
            {
                PlayerInfo playerInfo = GetPlayerInfo(e.Login);

                if (playerInfo == null)
                    return;

                NicknameResolverFactory.Instance.Set(e.Login, playerInfo.NickName);

                lock (_playerCountChangeLockObject)
                {
                    PlayersCount += 1;
                }

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
            if (e.Erroneous)
            {
                Logger.Error(string.Format("[Callbacks_PlayerDisconnect] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
                return;
            }

            RunCatchLog(() =>
            {
                Context.PlayerSettings.Remove(e.Login.ToLower());

                if (Settings.EnableLeaveMessage)
                {
                    string nickname = GetNickname(e.Login);

                    if (nickname != null)
                        SendFormattedMessage(Settings.LeaveMessage, "Nickname", StripTMColorsAndFormatting(nickname));
                }

                lock (_playerCountChangeLockObject)
                {
                    PlayersCount = (ushort) Math.Max(0, PlayersCount - 1);
                }
            }, "Error in Callbacks_PlayerDisconnect Method.", true);
        }

        private void Callbacks_BeginChallenge(object sender, BeginChallengeEventArgs e)
        {
            if (e.Erroneous)
            {
				Logger.Error(string.Format("[Callbacks_BeginChallenge] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
                return;
            }

            RunCatchLog(() =>
            {
                GetCurrentChallengeInfo(); // cache current challenge info
                GetServerOptions(); // cache current server options
                GetCurrentGameMode(); // cache current game mode
			}, "Error in Callbacks_BeginChallenge Method.", true);
        }
    }
}
