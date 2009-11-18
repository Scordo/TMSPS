using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.PluginSystem.Plugins.Restart
{
    public class RestartPlugin : TMSPSPlugin
    {
        #region Members

        private readonly object _lockObject = new object();

        #endregion


        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "RestartPlugin"; } }
        public override string Description { get { return "Provides an alternative voting-system for restarting."; } }
        public override string ShortName { get { return "Restart"; } }
        private HashSet<string> ProRestartLogins { get; set; }
        private HashSet<string> ConRestartLogins { get; set; }
        private HashSet<string> ProRestartCommands { get; set; }
        private HashSet<string> ConRestartCommands { get; set; }
        private RestartPluginSettings Settings { get; set; }
        private ushort AmountOfRestarts { get; set; }

        #endregion

        #region Constructor

        protected RestartPlugin(string pluginDirectory): base(pluginDirectory)
        {
            ProRestartLogins = new HashSet<string>();
            ConRestartLogins = new HashSet<string>();

            ProRestartCommands = new HashSet<string> { "res", "restart", "neustart", "res pls", "res please", "restart please", "restart pls" };
            ConRestartCommands = new HashSet<string> { "no res", "nores", "no restart", "kein restart", "kein neustart", "no res pls", "no res please", "no restart pls", "no restart please", "kein res" };
        }

        #endregion

        protected override void Init()
        {
            Settings = RestartPluginSettings.ReadFromFile(PluginSettingsFilePath);
            AmountOfRestarts = 0;

            Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
        }

        private void Callbacks_BeginRace(object sender, Communication.EventArguments.Callbacks.BeginRaceEventArgs e)
        {
            ProRestartLogins = new HashSet<string>();
            ConRestartLogins = new HashSet<string>();
        }

        private void Callbacks_EndRace(object sender, Communication.EventArguments.Callbacks.EndRaceEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(CheckForRestart);
        }

        private void CheckForRestart(object state)
        {
            RunCatchLog(()=>
			{
                if (PlayersCount == 0)
                    return;

                Thread.Sleep(Settings.FinishDelay);

                double currentProRestartVoteRatio = (double)ProRestartLogins.Count / PlayersCount;
                double currentConRestartVoteRatio = (double)ConRestartLogins.Count / PlayersCount;

                if (ProRestartLogins.Count > 0 && Settings.NoRestartPlayerLimit > 0 && PlayersCount >= Settings.NoRestartPlayerLimit)
                {
                    SendFormattedMessage("{[#ServerStyle]}>>{[#MessageStyle]} No restart is done because with more or equal than {[PlayerLimit]} no restart is allowed.", "PlayerLimit", Settings.NoRestartPlayerLimit.ToString());
                    AmountOfRestarts = 0;
                    return;
                }

                if (ProRestartLogins.Count > 0 && Settings.NoRestartVotesRatio > 0 && currentConRestartVoteRatio >= Settings.NoRestartVotesRatio)
                {
                    string configuredNoVoteRatio = (Settings.NoRestartVotesRatio * 100).ToString("F1");
                    string currentNoVoteRatio = (currentConRestartVoteRatio * 100).ToString("F1");

                    SendFormattedMessage("{[#ServerStyle]}>>{[#MessageStyle]} No restart is done because {[CurrentNoRestartVoteRatio]}% of the players voted against a restart ({[ConfiguredNoRestartVoteRatio]}% is the limit).", "CurrentNoRestartVoteRatio", currentNoVoteRatio, "ConfiguredNoRestartVoteRatio", configuredNoVoteRatio);
                    AmountOfRestarts = 0;
                    return;
                }

                if (Settings.SimpleRestartVoteRatio > 0 && currentProRestartVoteRatio >= Settings.SimpleRestartVoteRatio)
                {
                    RestartTrack();
                    return;
                }

                double effectiveRestartVoteRatio = currentProRestartVoteRatio - currentConRestartVoteRatio;

                if (ProRestartLogins.Count > 0 && Settings.AdvancedRestartVoteRatio > 0 && effectiveRestartVoteRatio < Settings.AdvancedRestartVoteRatio)
                {
                    string proRestart = (currentProRestartVoteRatio * 100).ToString("F1");
                    string conRestart = (currentConRestartVoteRatio * 100).ToString("F1");
                    string limit = (Settings.AdvancedRestartVoteRatio * 100).ToString("F1");

                    SendFormattedMessage("{[#ServerStyle]}>>{[#MessageStyle]} No restart is done because {[ConRestart]}% of the players voted against a restart and only {[ProRestart]}% of the players voted for a restart (Pro versus con percentages must be larger or equal to {[Limit]}%).", "ConRestart", conRestart, "ProRestart", proRestart, "Limit", limit);
                    AmountOfRestarts = 0;
                    return;
                }

                if (Settings.AdvancedRestartVoteRatio > 0 && effectiveRestartVoteRatio >= Settings.AdvancedRestartVoteRatio)
                    RestartTrack();
                else
                    AmountOfRestarts = 0;
            }, "Error in CheckForRestart Method.", true);
        }

        private void Callbacks_PlayerChat(object sender, Communication.EventArguments.Callbacks.PlayerChatEventArgs e)
        {
            if (e.IsServerMessage)
                return;

            RunCatchLog(()=>
			{
                string text = e.Text.ToLower(CultureInfo.InvariantCulture);

                ServerCommand command = ServerCommand.Parse(e.Text);
                if (ProRestartCommands.Contains(text) || command.Is(Command.ProRestart))
                {
                    if (Settings.RestartLimit > 0 && AmountOfRestarts >= Settings.RestartLimit)
                    {
                        SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}>{[#ErrorStyle]} Voting is disabled for this track because only {[Restarts]} restart(s) is/are allowed.", "Restarts", Settings.RestartLimit.ToString());
                        return;
                    }

                    if (ConsiderLogin(e.Login, true))
                    {
                        SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}>{[#MessageStyle]} Your vote for track restart was considered.");
                        SendStatisticsToLogin(e.Login);
                    }

                    return;
                }

                if ((ConRestartCommands.Contains(text) || command.Is(Command.ConRestart)))
                {
                    if (Settings.RestartLimit > 0 && AmountOfRestarts >= Settings.RestartLimit)
                    {
                        SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}>{[#ErrorStyle]} Voting is disabled for this track because only {[Restarts]} restart(s) is/are allowed.", "Restarts", Settings.RestartLimit.ToString());
                        return;
                    }

                    if (ConsiderLogin(e.Login, false))
                    {
                        SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}>{[#MessageStyle]} Your vote against track restart was considered.");
                        SendStatisticsToLogin(e.Login);
                    }
                }
            }, "Error in PlayerChat Callback.", true);
        }

        private void SendStatisticsToLogin(string login)
        {
            SendFormattedMessageToLogin(login, "{[#ServerStyle]}>{[#MessageStyle]} Restart-Info: {[RestartInfo]}", "RestartInfo", GetStatisticsText());
        }

        private string GetStatisticsText()
        {
            double currentProRestartVoteRatio = ((double)ProRestartLogins.Count / PlayersCount) * 100;
            double currentConRestartVoteRatio = ((double)ConRestartLogins.Count / PlayersCount) * 100;
            double notVoteRatio = 100 - currentProRestartVoteRatio - currentConRestartVoteRatio;

            return string.Format("{0}% voted for restart, {1}% voted against restart and {2}% did not vote.", currentProRestartVoteRatio.ToString("F1"), currentConRestartVoteRatio.ToString("F1"), notVoteRatio.ToString("F1"));
        }

        private void RestartTrack()
        {
            GenericResponse<bool> restartResponse = Context.RPCClient.Methods.RestartChallenge();

            if (restartResponse.Value)
            {
                SendFormattedMessage("{[#ServerStyle]}>>{[#MessageStyle]} Restarting current challenge due to player votes.");
                AmountOfRestarts++;
            }
            else
                AmountOfRestarts = 0;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
        }

        private bool ConsiderLogin(string login, bool proRestart)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            lock (_lockObject)
            {
                bool result = false;
                
                if (proRestart)
                {
                    result |= ConRestartLogins.Remove(login);
                    result |= ProRestartLogins.Add(login);
                }
                else
                {
                    result |= ProRestartLogins.Remove(login);
                    result |= ConRestartLogins.Add(login);
                }

                return result;
            }
        }

        public override IEnumerable<CommandHelp> CommandHelpList
        {
            get
            {
                return new[]
                {
                    new CommandHelp(Command.ProRestart, "Vote for a restart.", "/res", "/res"),
                    new CommandHelp(Command.ConRestart, "Vote against a restart.", "/nores", "/nores"),
                };
            }
        }
    }
}
