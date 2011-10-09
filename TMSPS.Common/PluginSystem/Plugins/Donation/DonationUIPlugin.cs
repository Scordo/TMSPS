using System;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.ManiaLinking;

namespace TMSPS.Core.PluginSystem.Plugins.Donation
{
    public class DonationUIPlugin : DonationPluginPlugin
    {
        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "Donation UI Plugin"; } }
        public override string Description { get { return "Displays a donations userinterface panel."; } }
        public override string ShortName { get { return "DonationUI"; } }
        public DonationUISettings Settings { get; set; }

        #endregion

        #region Constructor

        protected DonationUIPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion

        #region Methods

        protected override void Init()
        {
            Settings = DonationUISettings.ReadFromFile(PluginSettingsFilePath, ID);
            
            SendVotePanelToAll();

            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;

            if (!connectionLost)
                SendEmptyManiaLinkPage(DonationUISettings.PANEL_ID);
        }

        protected override void OnManiaLinkPageAnswer(string login, int playerID, TMAction action)
        {
            if (!action.IsAreaAction || action.AreaID != (byte)DonationUIArea.MainArea)
                return;

            byte index = Convert.ToByte(action.AreaActionID-1) ;

            HostPlugin.DonationFrom(login, (int)Settings.CopperValues[index]);
        }

        private void Callbacks_PlayerConnect(object sender, PlayerConnectEventArgs e)
        {
            SendVotePanelToLogin(e.Login);
        }

        private void SendVotePanelToAll()
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(Settings.PanelTemplate, 0, false);
        }

        private void SendVotePanelToLogin(string login)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, Settings.PanelTemplate, 0, false);
        }

        #endregion
    }
}