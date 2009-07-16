using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TMSPS.TRC.BL.Configuration;
using TMSPS.TRC.BL.Wpf.Helpers;

namespace TMSPS.TRC.Forms
{
    /// <summary>
    /// Interaction logic for CreateProfileForm.xaml
    /// </summary>
    public partial class CreateProfileForm
    {
        private CreateProfileContainer UIData { get; set; }
        public ProfileInfo CreatedProfile { get; private set; }

        public CreateProfileForm()
        {
            InitializeComponent();
            UIData = new CreateProfileContainer();
            DataContext = UIData;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IsInputValid();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsInputValid())
            {
                MessageBox.Show("Please fill all input fields correctly before proceeding.", "Input required!", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            CreatedProfile = new ProfileInfo(UIData.ProfileName, UIData.Password, GetNewProfileFilename());
            CreatedProfile.Save();
            DialogResult = true;
            Hide();
        }

        private void TRCBaseWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                DialogResult = false;
        }

        private bool IsInputValid()
        {
            ProfileNameTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            PasswordTextBox.GetBindingExpression(PasswordHelper.PasswordProperty).UpdateSource();
            PasswordRepeatTextBox.GetBindingExpression(PasswordHelper.PasswordProperty).UpdateSource();

            bool valid = true;

            if (Validation.GetErrors(ProfileNameTextBox).Count != 0)
                valid &= false;

            if (Validation.GetErrors(PasswordTextBox).Count != 0)
                valid &= false;

            if (Validation.GetErrors(PasswordRepeatTextBox).Count != 0)
                valid &= false;

            return valid;
        }

        private string GetNewProfileFilename()
        {
            return Path.Combine(Application.ProfilesDirectory, string.Format("{0}.{1}", Guid.NewGuid().ToString("N"), App.PROFILE_FILE_EXTENSION));
        }

        #region Embedded Types

        public class CreateProfileContainer : IDataErrorInfo
        {
            #region Properties

            public string ProfileName { get; set; }
            public string Password { get; set; }
            public string PasswordRepetition { get; set; }

            #endregion

            #region IDataErrorInfo Members

            string IDataErrorInfo.Error
            {
                get { return null; }
            }

            string IDataErrorInfo.this[string columnName]
            {
                get
                {
                    string result = null;

                    if (columnName == "ProfileName")
                    {
                        if (ProfileName.IsNullOrTimmedEmpty())
                            result = "Please provide a non empty profile name.";
                    }

                    if (columnName == "Password")
                    {
                        if (Password.IsNullOrTimmedEmpty())
                            result = "Please provide a non empty password.";
                    }

                    if (columnName == "PasswordRepetition")
                    {
                        if (PasswordRepetition.IsNullOrTimmedEmpty())
                            result = "Please provide a non empty password repetition.";

                        if (PasswordRepetition != Password)
                            result = "Password repetition does not match the password.";
                    }

                    return result;
                }
            }

            #endregion
        }

        #endregion
    }
}