using System;
using System.Collections.Generic;
using System.Globalization;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins
{
    public class TopRankingsPodiumPlugin : LocalRecordsPluginPlugin
    {
        #region Members

        private const string _linkPageID = "TopRankingsPodiumPluginPanelID";

        #endregion

        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "TopRankingsPodiumPlugin"; } }
        public override string Description { get { return "Shows top rankings when the podium screen is shown."; } }
        public override string ShortName { get { return "TopRankingsPodium"; } }
        private PodiumPluginSettings Settings { get; set; }

        #endregion

        #region Constructor

        protected TopRankingsPodiumPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion

        #region Methods

        protected override void Init()
        {
            Settings = PodiumPluginSettings.ReadFromFile(PluginSettingsFilePath, "Top Ranks", -40);
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
                List<PodiumPluginUIEntry> entries = HostPlugin.ServerRankRepository.GetList(Settings.MaxEntriesToShow).ConvertAll(ranking => new PodiumPluginUIEntry(ranking.Score.ToString("F1", CultureInfo.InvariantCulture), ranking.Nickname));
                Context.RPCClient.Methods.SendDisplayManialinkPage(PodiumPluginUI.GetRecordListManiaLinkPage(entries, _linkPageID, Settings), 0, false);
            }, "Error in Callbacks_EndRace Method.", true);
        }

        #endregion
    }
}