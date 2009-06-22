﻿using System;
using System.Collections.Generic;
using System.Globalization;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins
{
    public class TopWinnersPodiumPlugin : LocalRecordsPluginPlugin
    {
        #region Members

        private const string _linkPageID = "TopWinnersPodiumPluginPanelID";

        #endregion

        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "TopWinnersPodiumPlugin"; } }
        public override string Description { get { return "Shows top winners when the podium screen is shown."; } }
        public override string ShortName { get { return "TopWinnersPodium"; } }
        private PodiumPluginSettings Settings { get; set; }

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = PodiumPluginSettings.ReadFromFile(PluginSettingsFilePath, "Top Winners", -4);
            SendEmptyManiaLinkPage(_linkPageID);

			Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
			Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
        }

		private void Callbacks_BeginRace(object sender, Communication.EventArguments.Callbacks.BeginRaceEventArgs e)
        {
            RunCatchLog(() => SendEmptyManiaLinkPage(_linkPageID), "Error in Callbacks_BeginRace Method.", true);
        }

		private void Callbacks_EndRace(object sender, Communication.EventArguments.Callbacks.EndRaceEventArgs e)
        {
            RunCatchLog(() => 
            {
                List<PodiumPluginUIEntry> entries = HostPlugin.PlayerAdapter.DeserializeList(Settings.MaxEntriesToShow, PlayerSortOrder.Wins, false).ConvertAll(player => new PodiumPluginUIEntry(player.Wins.ToString("F0", CultureInfo.InvariantCulture), player.Nickname));
                Context.RPCClient.Methods.SendDisplayManialinkPage(PodiumPluginUI.GetRecordListManiaLinkPage(entries, _linkPageID, Settings), 0, false);
            }, "Error in Callbacks_EndRace Method.", true);
        }

        #endregion
    }
}