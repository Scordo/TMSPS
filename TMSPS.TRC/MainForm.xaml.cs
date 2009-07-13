using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TMSPS.TRC.BL.Configuration;
using TMSPS.TRC.BL.Wpf;
using TMSPS.TRC.Controls;
using TMSPS.TRC.Forms;

namespace TMSPS.TRC
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainForm
    {
        #region Constructor

        public MainForm()
        {
            InitializeComponent();
            LoginTextBox.TextChanged += LoginTextBox_TextChanged;
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty IsProfileSelectedProperty = DependencyProperty.Register("IsProfileSelected", typeof(bool), typeof(MainForm), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty CurrentProfileProperty = DependencyProperty.Register("CurrentProfile", typeof(Profile), typeof(MainForm), new FrameworkPropertyMetadata(null));

        public bool IsProfileSelected
        {
            get { return (bool)GetValue(IsProfileSelectedProperty); }
            set { SetValue(IsProfileSelectedProperty, value); }
        }

        public Profile CurrentProfile
        {
            get { return (Profile)GetValue(CurrentProfileProperty); }
            set { SetValue(CurrentProfileProperty, value); }
        }

        #endregion

        #region Public Methods

        public new void Log(string message)
        {
            string dateString = DateTime.Now.ToLongTimeString();
            LoginTextBox.AppendText(string.Format("[{0}] {1}{2}", dateString, message, Environment.NewLine));
        }

        #endregion

        #region Non Public Methods

        private void LoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LoginTextBox.LineCount > 0)
                LoginTextBox.ScrollToLine(LoginTextBox.LineCount - 1);
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            ShutDownApplication();
        }

        private void SelectProfileMenu_Click(object sender, RoutedEventArgs e)
        {
            SelectProfileForm selectProfileForm = new SelectProfileForm();
            selectProfileForm.FillUI();

            if (selectProfileForm.ShowDialog() != true)
                return;

            CurrentProfile = selectProfileForm.SelectedProfile;
            IsProfileSelected = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();

            ShutDownApplication();
        }

        private void ShutDownApplication()
        {
            Application.Shutdown();
        }

        private void Servers_ManageMenu_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentProfile == null)
                return;

            ManageServersForm manageServersForm = new ManageServersForm();
            manageServersForm.DataContext = Application.CurrentProfile.Servers.Clone();
            manageServersForm.ShowDialog();

            CurrentProfile.Servers.Clear();
            CurrentProfile.Servers.AddRange(manageServersForm.Servers);
            CurrentProfile.Save();
        }

        private void Servers_ConnectMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem) e.OriginalSource;
            ServerInfo serverInfo = (ServerInfo) menuItem.Header;

            ServerControl serverControl = new ServerControl {DataContext = new ServerControlDataContext(serverInfo.Clone())};
            serverControl.Close += ServerControl_Close;

            TabItem newTab = new TabItem { Header = serverInfo.Name, Content = serverControl };
            ServerForms.Items.Insert(0, newTab);
            ServerForms.SelectedIndex = 0;

            serverControl.ConnectToServer();
        }

        private void ServerControl_Close(object sender, EventArgs e)
        {
            ServerControl serverControl = (ServerControl) sender;
            ServerControlDataContext dataContext = (ServerControlDataContext) serverControl.DataContext;

            for (int i = 0; i < ServerForms.Items.Count; i++)
            {
                TabItem currentTab = (TabItem) ServerForms.Items[i];

                ServerControl currentServerControl = currentTab.Content as ServerControl;
                if (currentServerControl == null || currentServerControl.DataContext != dataContext)
                    continue;

                ServerForms.Items.RemoveAt(i);
                break;
            }
        }

        #endregion
    }
}
