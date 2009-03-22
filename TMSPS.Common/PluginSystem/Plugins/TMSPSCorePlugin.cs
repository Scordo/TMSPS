using System;
using System.Collections.Generic;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;
using Version=System.Version;

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
            GetPlayerList(); // cach all current player infos
            GetCurrentChallengeInfo(); // cache the current challenge info
            GetServerOptions(); // cache currenr server options
            GetCurrentGameMode(); // cache current game mode

            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
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
            Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
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
                    PlayerInfo playerInfo = GetPlayerInfoCached(e.Login);

                    if (playerInfo != null)
                        SendFormattedMessage(Settings.LeaveMessage, "Nickname", StripTMColorsAndFormatting(playerInfo.NickName));
                }

                RemoveCachedPlayerInfo(e.Login);
            }, "Error in Callbacks_PlayerDisconnect Method.", true);
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
                GetCurrentChallengeInfo(); // cache current challenge info
                GetServerOptions(); // cache current server options
                GetCurrentGameMode(); // cache current game mode
            }, "Error in Callbacks_BeginRace Method.", true);
        }
    }
}
