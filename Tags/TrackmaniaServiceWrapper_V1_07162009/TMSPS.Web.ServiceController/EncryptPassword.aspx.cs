using System;
using System.Web.Security;

namespace TMSPS.Web.ServiceController
{
    public partial class EncryptPassword : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            HashPasswordButton.Click += HashPasswordButton_Click;
        }

        private void HashPasswordButton_Click(object sender, EventArgs e)
        {
            if (!IsValid)
                return;

            HashLiteral.Text = FormsAuthentication.HashPasswordForStoringInConfigFile(PasswordTextBox.Text.Trim(), "SHA1");
        }
    }
}
