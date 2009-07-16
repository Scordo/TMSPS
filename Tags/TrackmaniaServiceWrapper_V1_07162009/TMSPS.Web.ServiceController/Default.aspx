<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="TMSPS.Web.ServiceController._Default" Title="Trackmania Web Service Controller" %>
<%@ Register TagPrefix="TMSPS" TagName="ServiceUIControl" Src="~/ServiceUIControl.ascx" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <meta http-equiv="refresh" content="5" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <div align="right">
            Logged in as: <asp:LoginName runat="server" /> (<asp:LoginStatus runat="server" />)
        </div>
    
        <TMSPS:ServiceUIControl runat="server" ID="TrackmaniaServerServiceControl" />
		<br />
		<br />
		<TMSPS:ServiceUIControl runat="server" ID="TMSPSServiceControl" />
    </div>
    </form>
</body>
</html>
