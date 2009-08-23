using System;
using System.Globalization;
using System.Threading;

namespace TMSPS.Core.PluginSystem.Plugins.Clock
{
    public class ClockPlugin : TMSPSPlugin
    {
        #region Constants

        private const string CLOCK_MANIA_LINK_PAGE_ID = "ClockPanelID";

        #endregion

        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); }}
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "ClockPlugin"; } }
        public override string Description { get { return "Shows a clock"; } }
        public override string ShortName { get { return "Clock"; } }
        public ClockPluginSettings Settings { get; private set; }
        private Timer ClockTimer { get; set; }

        #endregion

        #region Constructor

        protected ClockPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion

        #region Methods

        protected override void Init()
        {
            Settings = ClockPluginSettings.ReadFromFile(PluginSettingsFilePath);
            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            StartClockTimer();
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            StopClockTimer();

            if (!connectionLost)
            {
                SendEmptyManiaLinkPage(CLOCK_MANIA_LINK_PAGE_ID);
            }
        }

        private void Callbacks_PlayerConnect(object sender, Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            if (e.Handled)
                return;

            RunCatchLog(() => Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(e.Login, GetClockManiaLinkPage(), 0, false), "Error in Callbacks_PlayerConnect Method.", true);
        }

        private void UpdateClock(object state)
        {
            RunCatchLog(SendClockManiaLinkPageToAll, "Error in UpdateClock Method.", true);
        }

        private void SendClockManiaLinkPageToAll()
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(GetClockManiaLinkPage(), 0, false);
        }

        private string GetClockManiaLinkPage()
        {
            DateTime now = DateTime.Now;

            return ReplaceMessagePlaceHolders
            (
                Settings.ClockTemplate, 
                "ManiaLinkID", CLOCK_MANIA_LINK_PAGE_ID,
                "Hours1", now.Hour.ToString(CultureInfo.InvariantCulture),
                "Hours2", now.Hour.ToString("00",CultureInfo.InvariantCulture),
                "Minutes1", now.Minute.ToString(CultureInfo.InvariantCulture),
                "Minutes2", now.Minute.ToString("00",CultureInfo.InvariantCulture),
                "Half", now.Hour < 12 ? "AM" : "PM"
            );
        }

        private void StopClockTimer()
        {
            if (ClockTimer != null)
                ClockTimer.Dispose();
        }

        private void StartClockTimer()
        {
            ClockTimer = new Timer(UpdateClock, this, TimeSpan.FromMinutes(0), TimeSpan.FromMinutes(1));
        }

        #endregion
    }
}