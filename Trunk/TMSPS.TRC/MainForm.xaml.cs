using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMSPS.TRC.Forms;

namespace TMSPS.TRC
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainForm
    {
        public MainForm()
        {
            InitializeComponent();
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

            MessageBox.Show("Profile selected: " + selectProfileForm.SelectedProfile.Name);
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
            ManageServersForm manageServersForm = new ManageServersForm();
            manageServersForm.ShowDialog();
        }
    }
}
