using System;
using System.Collections.Generic;
using System.Threading;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ResponseHandling;
using PlayerInfo=TMSPS.Core.Communication.ProxyTypes.PlayerInfo;
using System.Linq;

namespace TMSPS.Core.PluginSystem.Plugins.IdleKick
{
    public class IdleKickPlugin : TMSPSPlugin
    {
        #region Non Public Members

        private readonly object _readWriteLockObject = new object();

        #endregion

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
            get { return "IdleKickPlugin"; }
        }

        public override string Description
        {
            get { return "Kicks players and/or spectators idling too long."; }
        }

        public override string ShortName
        {
            get { return "IdleKick"; }
        }

        public IdleKickPluginSettings Settings { get; private set; }

        private Dictionary<string, uint> LoginRounds { get; set; }
        private Dictionary<string, DateTime> LoginTimes { get; set; }
        private Timer IdleKickTimer { get; set; }

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = IdleKickPluginSettings.ReadFromFile(PluginSettingsFilePath);
            LoginRounds = new Dictionary<string, uint>();
            LoginTimes = new Dictionary<string, DateTime>();

            if (Settings.KickMode == IdleKickMode.TIME)
                IdleKickTimer = new Timer(KickIdlingPlayers, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

            List<PlayerInfo> playerList = GetPlayerList();

            if (playerList != null)
            {
                foreach (PlayerInfo playerInfo in playerList)
                {
                    ResetValues(playerInfo.Login);
                }
            }

            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.PlayerCheckpoint += Callbacks_PlayerCheckpoint;
            Context.RPCClient.Callbacks.PlayerFinish += Callbacks_PlayerFinish;
            Context.RPCClient.Callbacks.BeginChallenge += Callbacks_BeginChallenge;
        }

        private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
        {
            RunCatchLog(() => ResetValues(e.Login), "Error in Callbacks_PlayerConnect Method.", true);
        }

        private void Callbacks_PlayerDisconnect(object sender, PlayerDisconnectEventArgs e)
        {
            lock (_readWriteLockObject)
            {
                LoginRounds.Remove(e.Login);
                LoginTimes.Remove(e.Login);
            }
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            if (!Settings.ResetOnChat || e.IsServerMessage)
                return;

            RunCatchLog(() => ResetValues(e.Login), "Error in Callbacks_PlayerChat Method.", true);
        }

        private void Callbacks_PlayerCheckpoint(object sender, PlayerCheckpointEventArgs e)
        {
            if (!Settings.ResetOnCheckpoint)
                return;

            RunCatchLog(() => ResetValues(e.Login), "Error in Callbacks_PlayerCheckpoint Method.", true);
        }

        private void Callbacks_PlayerFinish(object sender, PlayerFinishEventArgs e)
        {
            if (!Settings.ResetOnFinish || e.TimeOrScore <= 0)
                return;

            RunCatchLog(() => ResetValues(e.Login), "Error in Callbacks_PlayerFinish Method.", true);
        }

        private void Callbacks_BeginChallenge(object sender, BeginChallengeEventArgs e)
        {
            RunCatchLog(() =>
            {
                if (Settings.KickMode != IdleKickMode.ROUNDS)
                    return;

                foreach (string login in LoginRounds.Keys.ToArray())
                    IncreaseLoginRounds(login);

                IEnumerable<KeyValuePair<string, uint>> loginsToKick;
                lock (_readWriteLockObject)
                {
                    loginsToKick = LoginRounds.Where(loginRound => loginRound.Value >= Settings.RoundsCount && !LoginHasRight(loginRound.Key, false, "NoIdleKick"));    
                }
                
                foreach (KeyValuePair<string, uint> loginRound in loginsToKick)
                {
                    if (!Settings.KickSpectators)
                    {
                        PlayerInfo playerInfo = GetPlayerInfo(loginRound.Key);

                        if (playerInfo == null || playerInfo.IsSpectator)
                        {
                            ResetLoginRounds(loginRound.Key);
                            continue;
                        }
                    }

                    KickLogin(loginRound.Key);
                }
            }, "Error in Callbacks_BeginChallenge Method.", true);
        }

        private void KickIdlingPlayers(object state)
        {
            RunCatchLog(() =>
            {
                IEnumerable<KeyValuePair<string, DateTime>> loginsToKick;

                DateTime now = DateTime.Now;
                lock (_readWriteLockObject)
                {
                    loginsToKick = LoginTimes.Where(loginTime => (now - loginTime.Value).TotalSeconds >= Settings.SecondsCount && !LoginHasRight(loginTime.Key, false, "NoIdleKick"));    
                }

                foreach (KeyValuePair<string, DateTime> loginTime in loginsToKick)
                {
                    if (!Settings.KickSpectators)
                    {
                        PlayerInfo playerInfo = GetPlayerInfo(loginTime.Key);

                        if (playerInfo == null || playerInfo.IsSpectator)
                        {
                            ResetLoginsTime(loginTime.Key);
                            continue;
                        }
                    }

                    KickLogin(loginTime.Key);
                }
            }, "Error in KickIdlingPlayers Method.", true);
        }

        private void KickLogin(string login)
        {
            PlayerInfo playerInfo = GetPlayerInfoCached(login);
            GenericResponse<bool> kickResponse = Context.RPCClient.Methods.Kick(login, Settings.PrivateKickMessage);

            if (kickResponse == null || !kickResponse.Value || playerInfo == null)
                return;

            SendFormattedMessage(Settings.PublicKickMessage, "Nickname", StripTMColorsAndFormatting(playerInfo.NickName));
        }

        private void ResetValues(string login)
        {
            if (Settings.KickMode == IdleKickMode.ROUNDS)
                ResetLoginRounds(login);
            else
                ResetLoginsTime(login);
        }

        private void ResetLoginRounds(string login)
        {
            lock (_readWriteLockObject)
            {
                LoginRounds[login] = 0;
            }
        }

        private void IncreaseLoginRounds(string login)
        {
            lock (_readWriteLockObject)
            {
                if (!LoginRounds.ContainsKey(login))
                    LoginRounds[login] = 0;

                LoginRounds[login] += 1;    
            }
        }

        private void ResetLoginsTime(string login)
        {
            lock (_readWriteLockObject)
            {
                LoginTimes[login] = DateTime.Now;
            }
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.PlayerDisconnect -= Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.PlayerCheckpoint -= Callbacks_PlayerCheckpoint;
            Context.RPCClient.Callbacks.PlayerFinish -= Callbacks_PlayerFinish;
            Context.RPCClient.Callbacks.BeginChallenge -= Callbacks_BeginChallenge;
        }

        #endregion
    }
}