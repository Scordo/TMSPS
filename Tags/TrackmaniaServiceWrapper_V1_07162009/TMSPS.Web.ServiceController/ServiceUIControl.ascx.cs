using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;

namespace TMSPS.Web.ServiceController
{
    public partial class ServiceUIControl : System.Web.UI.UserControl
    {
        private string ServiceName { get; set; }

        public void Bind(string serviceName, string description)
        {
            ServiceName = serviceName;
            DescriptionLiteral.Text = description;
            System.ServiceProcess.ServiceController serviceController = GetServiceController();

            UpdateUI(serviceController);
        }

        private void StartServerButton_Click(object sender, EventArgs e)
        {
            System.ServiceProcess.ServiceController trackmaniaServerServiceController = GetServiceController();

            if (trackmaniaServerServiceController.Status == ServiceControllerStatus.Stopped)
                trackmaniaServerServiceController.Start();

            Thread.Sleep(1000);
            UpdateUI();
        }

        private void StopServerButton_Click(object sender, EventArgs e)
        {
            System.ServiceProcess.ServiceController trackmaniaServerServiceController = GetServiceController();

            if (trackmaniaServerServiceController.Status == ServiceControllerStatus.Running)
                trackmaniaServerServiceController.Stop();

            Thread.Sleep(1000);
            UpdateUI();
        }

        private System.ServiceProcess.ServiceController GetServiceController()
        {
            string trackmaniaServerServiceName = ConfigurationManager.AppSettings[ServiceName];
            return new System.ServiceProcess.ServiceController(trackmaniaServerServiceName);
        }


        private void UpdateUI()
        {
            UpdateUI(GetServiceController());
        }

        private void UpdateUI(System.ServiceProcess.ServiceController serviceController)
        {
            bool trackmaniaServerServiceRunning = (serviceController.Status == ServiceControllerStatus.Running);

            StartButton.Enabled = !trackmaniaServerServiceRunning;
            StopButton.Enabled = trackmaniaServerServiceRunning;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            StartButton.Click += StartServerButton_Click;
            StopButton.Click += StopServerButton_Click;
        }
    }
}