using System;

namespace TMSPS.Core.PluginSystem.Plugins.StaticUI
{
    public class StaticUIPlugin : TMSPSPlugin
    {
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
            get { return "StaticUIPlugin"; }
        }

        public override string Description
        {
            get { return "Displays a static mania link page."; }
        }

        public override string ShortName
        {
            get { return "StaticUI"; }
        }

        public StaticUIPluginSettings Settings { get; private set; }

        #endregion

        #region Methods

        protected override void Init()
        {
            Settings = StaticUIPluginSettings.ReadFromFile(PluginSettingsFilePath);
            SendContentToAll();

            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace += Callbacks_EndRace;
        }

        private void Callbacks_EndRace(object sender, Communication.EventArguments.Callbacks.EndRaceEventArgs e)
        {
            if (Settings.HidOnFinish)
                SendEmptyManiaLinkPage("StaticUIPanel");
        }

        private void Callbacks_BeginRace(object sender, Communication.EventArguments.Callbacks.BeginRaceEventArgs e)
        {
            if (Settings.HidOnFinish)
                SendContentToAll();
        }

        private void SendContentToLogin(string login)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, Settings.ManiaLinkPageContent, 0, false);
        }

        private void SendContentToAll()
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(Settings.ManiaLinkPageContent, 0, false);
        }

        private void Callbacks_PlayerConnect(object sender, Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            SendContentToLogin(e.Login);
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
            Context.RPCClient.Callbacks.EndRace -= Callbacks_EndRace;
        }

        #endregion
    }
}