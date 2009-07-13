using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using TMSPS.Core.Common;
using TMSPS.Core.Communication;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.TRC.BL.Configuration;
using TMSPS.TRC.BL.Wpf;
using System.Linq;

namespace TMSPS.TRC.Controls
{
    /// <summary>
    /// Interaction logic for ServerControl.xaml
    /// </summary>
    public partial class ServerControl
    {
        #region Properties

        public ServerControlDataContext Context 
        { 
            get
            {
                Func<ServerControlDataContext> getDataContext = () => (ServerControlDataContext)DataContext;

                if (Dispatcher.CheckAccess())
                    return getDataContext();

                return (ServerControlDataContext) Dispatcher.Invoke(getDataContext, null);
            } 
        }

        public TrackManiaRPCClient RPCClient { get { return Context.RPCClient; } }
        public ServerInfo ServerInfo { get { return Context.ServerInfo; } }
        public Dictionary<UIAction, Action> ActionDictionary { get; set; }

        #endregion

        #region Events

        public event EventHandler Close;

        #endregion

        #region Constructor

        public ServerControl()
        {
            InitializeComponent();
            FillActionDictionary();
        }

        #endregion

        #region Public Methods

        public void ConnectToServer()
        {
            SelectedMenuItem(x => x.TargetTabType == typeof(TextBox));
            RPCClient.ReadyForSendingCommands += RPCClient_ReadyForSendingCommands;
            RPCClient.SocketError += RPCClient_SocketError;

            Log(string.Format("Connecting to '{0}' on port {1}...", Context.ServerInfo.Address, Context.ServerInfo.XmlRpcPort));

            RPCClient.Connect();   
        }

        #endregion

        private void Log(string message)
        {
            string dateString = DateTime.Now.ToLongTimeString();

            Action action = () => LogTextBox.AppendText(string.Format("[{0}] {1}{2}", dateString, message, Environment.NewLine));

            if (Dispatcher.CheckAccess())
                action();
            else
                Dispatcher.Invoke(action, null);
        }

        private void RPCClient_SocketError(object sender, Core.Communication.EventArguments.SocketErrorEventArgs e)
        {
            Log(string.Format("Socket error: {0}", e.SocketError));

            Action stopWork = OnStopWork;
            Dispatcher.Invoke(stopWork, null);
        }

        private void RPCClient_ReadyForSendingCommands(object sender, EventArgs e)
        {
            Log("Ready for sending commands...");

            Log("Trying to authenticate as SuperAdmin...");
            GenericResponse<bool> authResponse = RPCClient.Methods.Authenticate("SuperAdmin", ServerInfo.SuperAdminPassword);

            if (!authResponse.Erroneous && authResponse.Value)
            {
                Log("Authentication successfull!");

                Log("Trying to enable callbacks...");
                GenericResponse<bool> enableCallbacksResponse = RPCClient.Methods.EnableCallbacks(true);

                if (!enableCallbacksResponse.Erroneous && enableCallbacksResponse.Value)
                {
                    Log("Callbacks enabled successfully!");
                    
                    Action doWork = OnDoWork;
                    Dispatcher.Invoke(doWork, null);
                }
                else
                {
                    Log("Could not enable callbacks!");
                    RPCClient.Disconnect();
                }
            }
            else
            {
                Log("Authentication failed!");
                RPCClient.Disconnect();
            }
        }

        private void OnDoWork()
        {
            foreach (ServerControlTabContentControl tab in GetServerControlTabs())
                tab.DoWork();

            SelectedMenuItem(x => x.TargetTabType == typeof(ServerInfoTabContentControl));
            ToggleListBoxItemStatesAndTabs(true, new Type[] { }, new UIAction[] { });
        }

        private void OnStopWork()
        {
            foreach (ServerControlTabContentControl tab in GetServerControlTabs())
                tab.StopWork();

            SelectedMenuItem(x => x.TargetTabType == typeof (TextBox));
            ToggleListBoxItemStatesAndTabs(false, new [] {typeof(TextBox)}, new [] {UIAction.Close} );
        }

        private bool SelectedMenuItem(Predicate<ServerMenuEntry> predicate)
        {
            for (int i = 0; i < MenuListBox.Items.Count; i++ )
            {
                ServerMenuEntry serverMenuEntry = (ServerMenuEntry) MenuListBox.Items[i];

                if (predicate(serverMenuEntry))
                {
                    MenuListBox.SelectedIndex = i;
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<ServerControlTabContentControl> GetServerControlTabs()
        {
            foreach (TabItem tab in AreaTabControl.Items)
            {
                if (!(tab.Content is ServerControlTabContentControl))
                    continue;

                yield return (ServerControlTabContentControl) tab.Content;
            }
        }

        private void ToggleListBoxItemStatesAndTabs(bool enabled, IEnumerable<Type> excludedTypes, IEnumerable<UIAction> excludedActions)
        {
            if (excludedTypes == null)
                excludedTypes = new Type[] {};

            foreach (ServerMenuEntry serverMenuEntry in MenuListBox.Items)
            {
                if (serverMenuEntry.TargetTabType == null && serverMenuEntry.TargetAction == null)
                    continue;

                if (serverMenuEntry.TargetTabType != null && excludedTypes.Contains(serverMenuEntry.TargetTabType))
                    continue;

                if (serverMenuEntry.TargetAction != null && excludedActions.Contains(serverMenuEntry.TargetAction.Value))
                    continue;

                serverMenuEntry.Enabled = enabled;

                if (serverMenuEntry.TargetTabType == null)
                    continue;

                TabItem tabItem = GetTabItem(serverMenuEntry.TargetTabType);

                if (tabItem != null)
                    tabItem.IsEnabled = enabled;
            }
        }

        private bool SelectTab(Type contentType)
        {
            if (AreaTabControl == null)
                return false;

            for (int i = 0; i < AreaTabControl.Items.Count; i++)
            {
                TabItem tab = (TabItem) AreaTabControl.Items[i];
                
                if (tab.Content == null || tab.Content.GetType() != contentType)
                    continue;

                AreaTabControl.SelectedIndex = i;
                return true;
            }

            return false;
        }

        private TabItem GetTabItem(Type contentType)
        {
            if (AreaTabControl == null)
                return null;

            for (int i = 0; i < AreaTabControl.Items.Count; i++)
            {
                TabItem tab = (TabItem)AreaTabControl.Items[i];

                if (tab.Content == null || tab.Content.GetType() != contentType)
                    continue;

                return tab;
            }

            return null;
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            ServerMenuEntry currentEntry = (ServerMenuEntry) e.AddedItems[0];
            
            if (currentEntry.TargetTabType != null)
                SelectTab(currentEntry.TargetTabType);

            if (currentEntry.TargetAction.HasValue)
                ActionDictionary[currentEntry.TargetAction.Value]();
        }

        private void CloseInternal()
        {
            DetachEvents();

            if (RPCClient.IsConnected)
                RPCClient.Disconnect();

            if (Close != null)
                Close(this, EventArgs.Empty);
        }

        private void FillActionDictionary()
        {
            ActionDictionary = new Dictionary<UIAction, Action>
            {
                {UIAction.Close, CloseInternal}
            };
        }

        private void DetachEvents()
        {
            RPCClient.ReadyForSendingCommands -= RPCClient_ReadyForSendingCommands;
            RPCClient.SocketError -= RPCClient_SocketError;
        }

        public enum UIAction
        {
            Close
        }
    }


    public class ServerMenuEntry : NotifyPropertyChanged
    {
        #region Non Public Members

#pragma warning disable 649
        private string _name;
        private string _group;
        private Type _targetTabType;
        private bool _enabled;
#pragma warning restore 649

        private static ObservableCollection<ServerMenuEntry> _menuEntries = new ObservableCollection<ServerMenuEntry>
        {
                new ServerMenuEntry("Server info", "Server settings", typeof(ServerInfoTabContentControl), false),
                new ServerMenuEntry("Log", "Misc", typeof(TextBox), true),
                new ServerMenuEntry("Close", "Misc", ServerControl.UIAction.Close, true)
        };

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set { SetProperty(() => Name, () => _name, value); }
        }

        public string Group
        {
            get { return _group; }
            set { SetProperty(() => Group, () => _group, value); }
        }

        public Type TargetTabType
        {
            get { return _targetTabType; }
            set { SetProperty(() => TargetTabType, () => _targetTabType, value); }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { SetProperty(() => Enabled, () => _enabled, value); }
        }

        public static ObservableCollection<ServerMenuEntry> MenuEntries
        {
            get { return _menuEntries; }
            set { _menuEntries = value; }
        }

        public ServerControl.UIAction? TargetAction { get; set; }

        #endregion

        #region Constructors

        public ServerMenuEntry()
        {
            
        }

        public ServerMenuEntry(string name, string group, Type targetTabType, bool enabled)
        {
            Name = name;
            Group = group;
            TargetTabType = targetTabType;
            Enabled = enabled;
        }

        public ServerMenuEntry(string name, string group, ServerControl.UIAction targetAction, bool enabled)
        {
            Name = name;
            Group = group;
            TargetAction = targetAction;
            Enabled = enabled;
        }

        #endregion 
    }
}