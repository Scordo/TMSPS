<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Logon.aspx.cs" Inherits="TMSPS.Web.ServiceController.Logon" Title="Logon Page" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    
    <table align="center" cellspacing="2" cellpadding="2">
        <tr>
            <td colspan="2" align="center"><h3>Logon Page</h3></td>
        </tr>
        <tr>
            <td>
                Username:
            </td>
            <td>
                <asp:TextBox ID="UsernameTextBox" runat="server" />
            </td>
            <td>
                <asp:RequiredFieldValidator ControlToValidate="UsernameTextBox" Display="Dynamic" ErrorMessage="*" runat="server" />
            </td>
        </tr>
        <tr>
            <td>
                Password:
            </td>
            <td>
                <asp:TextBox ID="PasswordTextBox" TextMode="Password" runat="server" />
            </td>
            <td>
                <asp:RequiredFieldValidator ControlToValidate="PasswordTextBox" ErrorMessage="*" runat="server" />
            </td>
        </tr>
        <tr>
            <td />
            <td>
                <asp:CheckBox ID="PersistentCheckBox" runat="server" Text="Remember me?" />
            </td>
        </tr>
        <tr>
            <td />
            <td>
                <asp:Button ID="LogonButton" Text="Log On" runat="server" />
            </td>
        </tr>
    </table>
    
    <center>
        <p>
            <asp:Label ID="MessageLabel" ForeColor="red" runat="server" />
        </p>
    </center>
    </form>
    
    
</body>
</html>
