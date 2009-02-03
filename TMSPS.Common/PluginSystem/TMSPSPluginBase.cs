using System;
using System.IO;
using System.Reflection;
using TMSPS.Core.Logging;

namespace TMSPS.Core.PluginSystem
{
    public abstract class TMSPSPluginBase : ITMSPSPlugin
    {
        private PluginHostContext _context;

        public abstract Version Version { get; }
        public abstract string Author { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
		public abstract string ShortNameForLogging { get; }
		public IUILogger Logger { get; private set; }
        public PluginHostContext Context { get { return _context; } }

        protected static  string ApplicationDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        public void InitPlugin(PluginHostContext context, IUILogger logger)
        {
			if (logger == null)
				throw new ArgumentNullException("logger");

        	Logger = logger;
            _context = context;
            Init();
        }

        public void DisposePlugin()
        {
            Dispose();
        }

        protected abstract void Init();
        protected abstract void Dispose();

		protected virtual string GetConfigFilePath(string configFileName)
		{
			return Path.Combine(ApplicationDirectory, configFileName);
		}
    }
}