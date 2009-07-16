<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EncryptPassword.aspx.cs" Inherits="TMSPS.Web.ServiceController.EncryptPassword" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Password: <asp:TextBox runat="server" ID="PasswordTextBox" TextMode="Password"/>
        <asp:RequiredFieldValidator ControlToValidate="PasswordTextBox" Display="Dynamic" ErrorMessage="*" runat="server" /><br />
        Hash: <asp:Literal runat="server" ID="HashLiteral" /><br />
        <asp:Button runat="server" ID="HashPasswordButton" Text="encrypt password" />
    </div>
    </form>
</body>
</html>
