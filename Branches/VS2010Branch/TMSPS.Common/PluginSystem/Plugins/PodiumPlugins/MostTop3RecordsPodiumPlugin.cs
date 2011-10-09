using System;
using System.Collections.Generic;
using System.Globalization;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins
{
    public class MostTop3RecordsPodiumPlugin : LocalRecordsPluginPlugin
    {
        #region Members

        private const string _linkPageID = "MostTop3RecordsPodiumPluginPanelID";

        #endregion

        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "MostTop3RecordsPodiumPlugin"; } }
        public override string Description { get { return "Shows the players with the most top 3 records."; } }
        public override string ShortName { get { return "MostTop3RecordsPodium"; } }
        private PodiumPluginSettings Settings { get; set; }

        #endregion

        #region Constructor

        protected MostTop3RecordsPodiumPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion

        #region Methods

        protected override void Init()
        {
            Settings = PodiumPluginSettings.ReadFromFile(PluginSettingsFilePath, "Most Top3 Records", 24);
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
                List<PodiumPluginUIEntry> entries = HostPlugin.ChallengeRankRepository.GetChallengeRankLeadersHavingTopXRanks(Settings.MaxEntriesToShow, 3).ConvertAll(position => new PodiumPluginUIEntry(position.Amount.ToString("F0", CultureInfo.InvariantCulture), position.Nickname));
                Context.RPCClient.Methods.SendDisplayManialinkPage(PodiumPluginUI.GetRecordListManiaLinkPage(entries, _linkPageID, Settings), 0, false);
            }, "Error in Callbacks_EndRace Method.", true);
        }

        #endregion
    }
}