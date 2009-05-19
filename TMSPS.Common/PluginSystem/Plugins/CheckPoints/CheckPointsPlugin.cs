using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMSPS.Core.Communication.EventArguments.Callbacks;

namespace TMSPS.Core.PluginSystem.Plugins.CheckPoints
{
    public class CheckPointsPlugin : TMSPSPlugin
    {
        #region Members

        private const string MANIA_LINK_PAGE_ID = "CheckPointsPanelID"; 
        private readonly object _cpReadWriteLock = new object();

        #endregion

        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "CheckPoints Plugin"; } }
        public override string Description { get { return "Displays statistics and local records in a userinterface."; } }
        public override string ShortName { get { return "CheckPoints"; } }
        public CheckPointsSettings Settings { get; set; }
        private Dictionary<string, Dictionary<int, int>> BestCheckPoints { get; set; }
        

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = CheckPointsSettings.ReadFromFile(PluginSettingsFilePath);
            BestCheckPoints = new Dictionary<string, Dictionary<int, int>>();

            Context.RPCClient.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.BeginChallenge += Callbacks_BeginChallenge;
            Context.RPCClient.Callbacks.EndChallenge += Callbacks_EndChallenge;
            Context.RPCClient.Callbacks.PlayerCheckpoint += Callbacks_PlayerCheckpoint;
            Context.RPCClient.Callbacks.PlayerFinish += Callbacks_PlayerFinish;
            Context.RPCClient.Callbacks.PlayerInfoChanged += Callbacks_PlayerInfoChanged;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerDisconnect -= Callbacks_PlayerDisconnect;
            Context.RPCClient.Callbacks.BeginChallenge -= Callbacks_BeginChallenge;
            Context.RPCClient.Callbacks.EndChallenge -= Callbacks_EndChallenge;
            Context.RPCClient.Callbacks.PlayerCheckpoint -= Callbacks_PlayerCheckpoint;
            Context.RPCClient.Callbacks.PlayerFinish -= Callbacks_PlayerFinish;
            Context.RPCClient.Callbacks.PlayerInfoChanged -= Callbacks_PlayerInfoChanged;
        }

        private int? GetDiff(string login, int checkpointIndex, int time, bool isFinish)
        {
            lock (_cpReadWriteLock)
            {
                if (!BestCheckPoints.ContainsKey(login))
                {
                    BestCheckPoints[login] = new Dictionary<int, int> { { checkpointIndex, time } };
                    return null;
                }

                if (isFinish)
                    checkpointIndex = BestCheckPoints[login].Keys.Max();

                if (!BestCheckPoints[login].ContainsKey(checkpointIndex))
                {
                    BestCheckPoints[login][checkpointIndex] = time;
                    return null;
                }

                int diff = BestCheckPoints[login][checkpointIndex] - time;

                if (diff > 0)
                    BestCheckPoints[login][checkpointIndex] = time;

                return diff;
            }
        }

        private string GetManiaLinkPage(int diff, int checkpointIndex, bool isFinish)
        {
            string sign = string.Empty;
            string textStyle = Settings.SuperiorTimeTextStyle;

            if (diff < 0)
            {
                sign = "+";
                textStyle = Settings.InferiorTimeTextStyle;
            }

            if (diff > 0)
                sign = "-";

            double secondsDiff = diff/1000d;
            string diffText = string.Concat(textStyle, sign, Math.Abs(secondsDiff).ToString("F2", CultureInfo.InvariantCulture));
            string cpText = string.Format("{0}{1}:", isFinish ? "FCP" : "CP", isFinish ? string.Empty : " " +(checkpointIndex + 1));

            return FormatMessage(Settings.Template, "ManiaLinkID", MANIA_LINK_PAGE_ID, "CheckPointText", cpText, "DiffText", diffText);
        }

        private void SendCheckPointUIToLogin(string login, int diff, int checkpointIndex, bool isFinish)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetManiaLinkPage(diff, checkpointIndex, isFinish), Convert.ToInt32(Settings.TimeoutInSeconds * 1000), false);
        }

        private void Callbacks_PlayerDisconnect(object sender, PlayerDisconnectEventArgs e)
        {
            BestCheckPoints.Remove(e.Login);
        }

        private void Callbacks_BeginChallenge(object sender, BeginChallengeEventArgs e)
        {
            lock (_cpReadWriteLock)
            {
                BestCheckPoints = new Dictionary<string, Dictionary<int, int>>();
            }
        }

        private void Callbacks_EndChallenge(object sender, EndChallengeEventArgs e)
        {
            SendEmptyManiaLinkPage(MANIA_LINK_PAGE_ID);
        }

        private void Callbacks_PlayerCheckpoint(object sender, PlayerCheckpointEventArgs e)
        {
            if (e.TimeOrScore <= 0)
            {
                SendEmptyManiaLinkPageToLogin(e.Login, MANIA_LINK_PAGE_ID);
                return;
            }

            int? diff = GetDiff(e.Login, e.CheckpointIndex, e.TimeOrScore, false);

            if (diff.HasValue)
                SendCheckPointUIToLogin(e.Login, diff.Value, e.CheckpointIndex, false);
            else
                SendEmptyManiaLinkPageToLogin(e.Login, MANIA_LINK_PAGE_ID);
        }

        private void Callbacks_PlayerFinish(object sender, PlayerFinishEventArgs e)
        {
            if (e.TimeOrScore <= 0)
            {
                SendEmptyManiaLinkPageToLogin(e.Login, MANIA_LINK_PAGE_ID);
                return;
            }

            int? diff = GetDiff(e.Login, 0, e.TimeOrScore, true);
            SendCheckPointUIToLogin(e.Login, diff.Value, 0, true);
        }

        private void Callbacks_PlayerInfoChanged(object sender, PlayerInfoChangedEventArgs e)
        {
            if (e.PlayerInfo.SpectatorStatus != 0)
                SendEmptyManiaLinkPageToLogin(e.PlayerInfo.Login, MANIA_LINK_PAGE_ID);
        }

        #endregion
    }
}