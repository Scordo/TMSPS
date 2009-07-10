using System.Windows;
using System.Windows.Controls;
using TMSPS.TRC.BL.Configuration;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

            MessageBox.Show("Selected server address: " + serverInfo.Address);
        }
    }
}
