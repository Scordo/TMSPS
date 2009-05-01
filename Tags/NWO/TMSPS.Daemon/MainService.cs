using System.ServiceProcess;

namespace TMSPS.Daemon
{
    public partial class MainService : ServiceBase
    {
		private readonly MainDaemon _mainDaemon = new MainDaemon();

        public MainService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
			_mainDaemon.Start();
        }

        protected override void OnStop()
        {
        	_mainDaemon.Stop();
        }
    }
}