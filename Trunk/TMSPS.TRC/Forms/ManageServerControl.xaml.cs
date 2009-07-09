using System.Windows;
using System.Windows.Controls;
using TMSPS.TRC.BL.Configuration;
using TMSPS.TRC.BL.Wpf.Helpers;

namespace TMSPS.TRC.Forms
{
    /// <summary>
    /// Interaction logic for ManageServerControl.xaml
    /// </summary>
    public partial class ManageServerControl : UserControl
    {
        public static readonly DependencyProperty WasModifiedProperty = DependencyProperty.Register("WasModified", typeof(bool), typeof(ManageServerControl), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty HasChangedAndIsValidProperty = DependencyProperty.Register("HasChangedAndIsValid", typeof(bool), typeof(ManageServerControl), new FrameworkPropertyMetadata(false));

        public bool WasModified
        {
            get { return (bool)GetValue(WasModifiedProperty); }
            set { SetValue(WasModifiedProperty, value); }
        }

        public bool HasChangedAndIsValid
        {
            get { return (bool)GetValue(HasChangedAndIsValidProperty); }
            set { SetValue(HasChangedAndIsValidProperty, value); }
        }

        private ServerInfo UnmodifiedServerInfo { get; set;}
        private ServerInfo CurrentServerInfo { get { return (ServerInfo)DataContext; } }
        
        public ManageServerControl()
        {
            InitializeComponent();
            DataContextChanged += ManageServerControl_DataContextChanged;
        }

        private void CurrentServerInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (UnmodifiedServerInfo == null && CurrentServerInfo == null)
            {
                WasModified = false;
                return;
            }

            if (UnmodifiedServerInfo == null)
            {
                WasModified = true;
                return;
            }

            WasModified = !UnmodifiedServerInfo.Equals(CurrentServerInfo);
            HasChangedAndIsValid = WasModified && Validate();
        }

        private void ManageServerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WasModified = false;
            HasChangedAndIsValid = false;

            if (DataContext == null || !(DataContext is ServerInfo))
                return;

            CurrentServerInfo.PropertyChanged += CurrentServerInfo_PropertyChanged;
            UnmodifiedServerInfo = ((ServerInfo) DataContext).Clone();

        }

        public void Reset()
        {
            if (CurrentServerInfo == null)
                return;

            DataContext = UnmodifiedServerInfo.Clone();
        }

        public ServerInfo GetValidServerInfo()
        {
            return IsValid() ? CurrentServerInfo.Clone() : null;
        }

        public bool Validate()
        {
            ServernameTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            ServerAddressTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            PortTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            PasswordTextBox.GetBindingExpression(PasswordHelper.PasswordProperty).UpdateSource();
            DescriptionTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            
            return IsValid();
        }

        public bool IsValid()
        {
            bool valid = true;

            if (Validation.GetErrors(ServernameTextBox).Count != 0)
                valid &= false;

            if (Validation.GetErrors(ServerAddressTextBox).Count != 0)
                valid &= false;

            if (Validation.GetErrors(PortTextBox).Count != 0)
                valid &= false;

            if (Validation.GetErrors(PasswordTextBox).Count != 0)
                valid &= false;

            if (Validation.GetErrors(DescriptionTextBox).Count != 0)
                valid &= false;

            return valid;
        }
    }
}
