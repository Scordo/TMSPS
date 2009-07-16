using System;
using System.Configuration;
using System.Windows.Forms;
using TMSPS.Core.Common;
using TMSPS.Core.Communication;

namespace TMSPS.TestClient
{
    public partial class MainForm : Form
    {
        private readonly TrackManiaRPCClient Client = new TrackManiaRPCClient(ConfigurationManager.AppSettings["ServerAddress"], Convert.ToInt32(ConfigurationManager.AppSettings["ServerXMLPort"]));

        private void Client_ReadyForSendingCommands(object sender, EventArgs e)
        {
            Log(Client.Methods.Authenticate("SuperAdmin", ConfigurationManager.AppSettings["SuperAdminPassword"]).GetDumpString());
        }

        public MainForm()
        {
            InitializeComponent();
            Client.ServerClosedConnection += Client_ServerClosedConnection;
            Client.SocketError += Client_SocketError;
            Client.Connected += Client_Connected;
            Client.ReadyForSendingCommands += Client_ReadyForSendingCommands;
            Client.Callbacks.ServerStart += Callbacks_ServerStart;
            Client.Callbacks.ServerStop += Callbacks_ServerStop;
            Client.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
            Client.Callbacks.PlayerDisconnect += Callbacks_PlayerDisconnect;
            Client.Callbacks.PlayerChat += Callbacks_PlayerChat;
        }

        private void Callbacks_PlayerChat(object sender, Core.Communication.EventArguments.Callbacks.PlayerChatEventArgs e)
        {
            Log("Chat: "+e.GetDumpString());
        }

        

        private void Callbacks_PlayerDisconnect(object sender, Core.Communication.EventArguments.Callbacks.PlayerDisconnectEventArgs e)
        {
            Log("Player disconnected: " + e.GetDumpString());
        }

        private void Callbacks_PlayerConnect(object sender, Core.Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            Log("Player connected: "+e.GetDumpString());
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            Log("connection to server established");
            ConnectButton.BeginInvoke(new MethodInvoker(() => { ConnectButton.Enabled = false; }));
            DisconnectButton.BeginInvoke(new MethodInvoker(() => { DisconnectButton.Enabled = true; }));
        }

        private void Callbacks_ServerStop(object sender, EventArgs e)
        {
            Log("Server stopped");
        }

        private void Callbacks_ServerStart(object sender, EventArgs e)
        {
            Log("Server started");
        }

        private void Client_SocketError(object sender, Core.Communication.EventArguments.SocketErrorEventArgs e)
        {
            Log("Connection Error: "+e.SocketError);
            ConnectButton.BeginInvoke(new MethodInvoker(() => { ConnectButton.Enabled = true; }));
            DisconnectButton.BeginInvoke(new MethodInvoker(() => { DisconnectButton.Enabled = false; }));
        }

        private void Client_ServerClosedConnection(object sender, EventArgs e)
        {
            Log("Server Closed connection...");
            ConnectButton.BeginInvoke(new MethodInvoker(() => { ConnectButton.Enabled = true; }));
            DisconnectButton.BeginInvoke(new MethodInvoker(() => { DisconnectButton.Enabled = false; }));
        }

        private void Log(string message)
        {
            LogTextBox.BeginInvoke(new MethodInvoker(() =>
            {
                LogTextBox.SelectionStart = 0;
                LogTextBox.SelectedText = message + Environment.NewLine;
            }));
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            ConnectButton.Enabled = false;

            Client.Connect();
        }

        private void DiconnectButton_Click(object sender, EventArgs e)
        {
            DisconnectButton.Enabled = false;
            Client.Disconnect();
        }

        private void GetVersion_Click(object sender, EventArgs e)
        {
            Log(Client.Methods.GetVersion().GetDumpString());
        }

        private void checkConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log(Client.Methods.GetBanList(500, 0).GetDumpString());
        }

        private void enableCallbacksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log(Client.Methods.EnableCallbacks(true).GetDumpString());
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
    }
}
