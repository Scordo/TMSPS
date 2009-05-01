﻿using System;
using System.Collections.Generic;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;
using System.Globalization;

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins
{
    public class HoursPlayedPodiumPlugin : LocalRecordsPluginPlugin
    {
        #region Members

        private readonly string _linkPageID = "HoursPlayedPodiumPluginPanelID"; //Guid.NewGuid().ToString("N");

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
            get { return "HoursPlayedPodiumPlugin"; }
        }

        public override string Description
        {
            get { return "Shows top hours played when the podium screen is shown."; }
        }

        public override string ShortName
        {
            get { return "HoursPlayedPodium"; }
        }

        private PodiumPluginSettings Settings
        {
            get; set;
        }

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = PodiumPluginSettings.ReadFromFile(PluginSettingsFilePath, "Hours Played", -24);
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
            List<PodiumPluginUIEntry> entries = HostPlugin.PlayerAdapter.DeserializeList(Settings.MaxEntriesToShow, PlayerSortOrder.TimePlayed, false).ConvertAll(player => new PodiumPluginUIEntry(Math.Floor(player.TimePlayed.TotalHours).ToString("F0", CultureInfo.InvariantCulture) + "h", player.Nickname));
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
