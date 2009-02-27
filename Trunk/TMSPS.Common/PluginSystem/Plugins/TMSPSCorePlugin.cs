using System;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;

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
            get { return "CorePlugin"; }
        }

        #endregion

        protected override void Init()
        {
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
                HandleCommand(command);
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
                    Context.RPCClient.Methods.Kick(e.Login, "Please provide a nickname!");
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
