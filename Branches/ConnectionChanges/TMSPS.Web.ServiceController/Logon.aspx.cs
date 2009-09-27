using System;
using System.Web.Security;

namespace TMSPS.Web.ServiceController
{
    public partial class Logon : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
                Response.Redirect("~/default.aspx");
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            LogonButton.Click += LogonButton_Click;
        }

        private void LogonButton_Click(object sender, EventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordTextBox.Text.Trim();

            if (FormsAuthentication.Authenticate(username, password))
                FormsAuthentication.RedirectFromLoginPage(username, PersistentCheckBox.Checked);
            else
                MessageLabel.Text = "Login failed. Please check your user name and password and try again.";
        }
    }
}