using System;
using System.Collections.Generic;
using System.Globalization;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins
{
    public class TopRankingsPodiumPlugin : LocalRecordsPluginPlugin
    {
        #region Members

        private readonly string _linkPageID = "TopRankingsPodiumPluginPanelID"; //Guid.NewGuid().ToString("N");

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
            get { return "TopRankingsPodiumPlugin"; }
        }

        public override string Description
        {
            get { return "Shows top rankings when the podium screen is shown."; }
        }

        public override string ShortName
        {
            get { return "TopRankingsPodium"; }
        }

        private PodiumPluginSettings Settings
        {
            get; set;
        }

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = PodiumPluginSettings.ReadFromFile(PluginSettingsFilePath, "Top Ranks", -40);
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
            List<PodiumPluginUIEntry> entries = HostPlugin.RankingAdapter.Deserialize_List(Settings.MaxEntriesToShow).ConvertAll(ranking => new PodiumPluginUIEntry(ranking.Score.ToString("F1", CultureInfo.InvariantCulture), ranking.Nickname));
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
