using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Input;
using TMSPS.TRC.BL.Configuration;
using Path=System.IO.Path;
using System.Linq;

namespace TMSPS.TRC.Forms
{
    /// <summary>
    /// Interaction logic for SelectProfileForm.xaml
    /// </summary>
    public partial class SelectProfileForm
    {
        private List<ProfileInfo> _loadedProfiles;
        public Profile SelectedProfile { get; private set; }

        public SelectProfileForm()
        {
            InitializeComponent();
        }

        public void FillUI()
        {
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string profilePath = Path.Combine(appPath, "Profiles");
            const string profileFileExtension = "profile";
            _loadedProfiles = ProfileInfo.GetProfileInfoList(profilePath, profileFileExtension);

            FillUI(_loadedProfiles);
        }

        private void FillUI(IEnumerable<ProfileInfo> profiles)
        {
            PasswordExpander.IsExpanded = false;
            PasswordTextBox.Password = null;
            ProfileComboBox.Items.Clear();

            foreach (ProfileInfo profileInfo in profiles.OrderBy(x => x.Name))
                ProfileComboBox.Items.Add(profileInfo);

            ProfileComboBox.Items.Add("<< Create a new profile >>");
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordTextBox.Password.IsNullOrTimmedEmpty())
            {
                MessageBox.Show("Please type in a password to open the profile.", "Input required!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string password = PasswordTextBox.Password.Trim();
            ProfileInfo selectedProfileInfo = (ProfileInfo) ProfileComboBox.SelectedItem;
            Profile loadedProfile = Profile.ReadFromFile(selectedProfileInfo.FilePath, password);

            if (loadedProfile == null)
            {
                MessageBox.Show("The provided password is wrong for the selected profile.", "Wrong password!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            loadedProfile.Password = password;
            SelectedProfile = loadedProfile;
            DialogResult = true;
        }

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileComboBox.Items.Count == 0)
                return;

            SelectButton.IsEnabled = (ProfileComboBox.SelectedItem is ProfileInfo);

            if (ProfileComboBox.SelectedItem is ProfileInfo)
            {
                PasswordExpander.IsExpanded = true;
                PasswordTextBox.Focus();
                return;
            }

            CreateProfileForm createProfileForm = new CreateProfileForm();
            createProfileForm.ShowDialog();
            createProfileForm.Close();

            if (createProfileForm.CreatedProfile != null)
                _loadedProfiles.Add(createProfileForm.CreatedProfile);
            
            FillUI(_loadedProfiles);
        }

        private void TRCBaseWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                DialogResult = false;
        }
    }
}