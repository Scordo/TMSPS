using System;
using TMSPS.Core.Communication.EventArguments.Callbacks;

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
            Context.RPCClient.Callbacks.BeginChallenge += Callbacks_BeginChallenge;
            Context.RPCClient.Callbacks.EndChallenge += Callbacks_EndChallenge;
        }

        private void Callbacks_EndChallenge(object sender, EndChallengeEventArgs e)
        {
            if (Settings.HidOnFinish)
                SendEmptyManiaLinkPage("StaticUIPanel");
        }

        private void Callbacks_BeginChallenge(object sender, BeginChallengeEventArgs e)
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

        private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
        {
            SendContentToLogin(e.Login);
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;
            Context.RPCClient.Callbacks.BeginChallenge -= Callbacks_BeginChallenge;
            Context.RPCClient.Callbacks.EndChallenge -= Callbacks_EndChallenge;
        }

        #endregion
    }
}