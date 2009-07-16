using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;

namespace TMSPS.Web.ServiceController
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            TrackmaniaServerServiceControl.Bind("TrackmaniaServerServiceName", "Trackmania Server");

            string showTMSPSControllerConfigString = ConfigurationManager.AppSettings["ShowTMSPSController"];
            bool showTMSPSController = (showTMSPSControllerConfigString == null) || string.Compare("true", showTMSPSControllerConfigString, true) == 0;

            if (showTMSPSController)
                TMSPSServiceControl.Bind("TMSPSServiceName", "TMSPS");

            TMSPSServiceControl.Visible = showTMSPSController;
        }
    }
}
