using System;
using System.Collections.Generic;
using System.Globalization;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins
{
    public class MostTop3RecordsPodiumPlugin : LocalRecordsPluginPlugin
    {
        #region Members

        private readonly string _linkPageID = "MostTop3RecordsPodiumPluginPanelID"; //Guid.NewGuid().ToString("N");

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
            get { return "MostTop3RecordsPodiumPlugin"; }
        }

        public override string Description
        {
            get { return "Shows the players with the most top 3 records."; }
        }

        public override string ShortName
        {
            get { return "MostTop3RecordsPodium"; }
        }

        private PodiumPluginSettings Settings
        {
            get; set;
        }

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = PodiumPluginSettings.ReadFromFile(PluginSettingsFilePath, "Most Top3 Records", 24);
            SendEmptyManiaLinkPage(_linkPageID);

            Context.RPCClient.Callbacks.BeginChallenge += Callbacks_BeginChallenge;
            Context.RPCClient.Callbacks.EndChallenge += Callbacks_EndChallenge;
        }

        private void Callbacks_BeginChallenge(object sender, Communication.EventArguments.Callbacks.BeginChallengeEventArgs e)
        {
            SendEmptyManiaLinkPage(_linkPageID);
        }

        private void Callbacks_EndChallenge(object sender, Communication.EventArguments.Callbacks.EndChallengeEventArgs e)
        {
            List<PodiumPluginUIEntry> entries = HostPlugin.RankingAdapter.DeserializeListByMost(Settings.MaxEntriesToShow, 3).ConvertAll(position => new PodiumPluginUIEntry(position.Amount.ToString("F0", CultureInfo.InvariantCulture), position.Nickname));
            Context.RPCClient.Methods.SendDisplayManialinkPage(PodiumPluginUI.GetRecordListManiaLinkPage(entries, _linkPageID, Settings), 0, false);
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.BeginChallenge -= Callbacks_BeginChallenge;
            Context.RPCClient.Callbacks.EndChallenge -= Callbacks_EndChallenge;
        }

        #endregion
    }
}
