using System;
using System.Collections.Generic;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;
using System.Globalization;

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins
{
    public class HoursPlayedPodiumPlugin : LocalRecordsPluginPlugin
    {
        #region Members

        private const string _linkPageID = "HoursPlayedPodiumPluginPanelID";

        #endregion

        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "HoursPlayedPodiumPlugin"; } }
        public override string Description { get { return "Shows top hours played when the podium screen is shown."; } }
        public override string ShortName { get { return "HoursPlayedPodium"; } }
        private PodiumPluginSettings Settings { get; set; }

        #endregion

        #region Constructor

        protected HoursPlayedPodiumPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion

        #region Methods

        protected override void Init()
        {
            Settings = PodiumPluginSettings.ReadFromFile(PluginSettingsFilePath, "Hours Played", -24);
            SendEmptyManiaLinkPage(_linkPageID);

			Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
			Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;

            if (!connectionLost)
                SendEmptyManiaLinkPage(_linkPageID);
        }

		private void Callbacks_BeginRace(object sender, Communication.EventArguments.Callbacks.BeginRaceEventArgs e)
        {
            RunCatchLog(() => SendEmptyManiaLinkPage(_linkPageID), "Error in Callbacks_BeginRace Method.", true);
        }

		private void Callbacks_EndRace(object sender, Communication.EventArguments.Callbacks.EndRaceEventArgs e)
        {
            RunCatchLog(() => 
            {
                List<PodiumPluginUIEntry> entries = HostPlugin.PlayerRepository.GetList(Settings.MaxEntriesToShow, PlayerSortOrder.TimePlayed, false).ConvertAll(Player.FromPlayerEntity).ConvertAll(player => new PodiumPluginUIEntry(Math.Floor(player.TimePlayed.TotalHours).ToString("F0", CultureInfo.InvariantCulture) + "h", player.Nickname));
                Context.RPCClient.Methods.SendDisplayManialinkPage(PodiumPluginUI.GetRecordListManiaLinkPage(entries, _linkPageID, Settings), 0, false);
            }, "Error in Callbacks_EndRace Method.", true);
        }

        #endregion
    }
}