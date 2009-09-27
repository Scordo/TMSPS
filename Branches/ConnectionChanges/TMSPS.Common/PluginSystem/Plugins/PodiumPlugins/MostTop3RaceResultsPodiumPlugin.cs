using System;
using System.Collections.Generic;
using System.Globalization;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins
{
    public class MostTop3RaceResultsPodiumPlugin : LocalRecordsPluginPlugin
    {
        #region Members

        private const string _linkPageID = "MostTop3RaceResultsPodiumPluginPanelID";

        #endregion

        #region Constructor

        protected MostTop3RaceResultsPodiumPlugin(string pluginDirectory)
            : base(pluginDirectory)
        {

        }

        #endregion

        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "MostTop3RaceResultsPodiumPlugin"; } }
        public override string Description { get { return "Shows the players with the most race result within top 3."; } }
        public override string ShortName { get { return "MostTop3RaceResultsPodium"; } }
        private PodiumPluginSettings Settings { get; set; }

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = PodiumPluginSettings.ReadFromFile(PluginSettingsFilePath, "Most Top3 Race Results", 8);
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
                List<PodiumPluginUIEntry> entries;
                using (IPositionAdapter positionAdapter = HostPlugin.AdapterProvider.GetPositionAdapter())
                {
                    entries = positionAdapter.DeserializeListByMost(Settings.MaxEntriesToShow, 3).ConvertAll(position => new PodiumPluginUIEntry(position.Amount.ToString("F0", CultureInfo.InvariantCulture), position.Nickname));
                }

                Context.RPCClient.Methods.SendDisplayManialinkPage(PodiumPluginUI.GetRecordListManiaLinkPage(entries, _linkPageID, Settings), 0, false);
            }, "Error in Callbacks_EndRace Method.", true);
        }

        #endregion
    }
}