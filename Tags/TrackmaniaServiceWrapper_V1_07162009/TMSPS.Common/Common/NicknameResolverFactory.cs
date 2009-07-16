using System;
using TMSPS.Core.PluginSystem;
using TMSPS.Core.PluginSystem.Plugins;

namespace TMSPS.Core.Common
{
    public class NicknameResolverFactory
    {
        #region Non Public Members

        private static readonly object _lockObject = new object();

        #endregion

        #region Properties

        public static INicknameResolver Instance
        {
            get; private set;
        }

        #endregion

        #region Public Methods

        public static void CreateSingleInstance(TMSPSCorePluginSettings settings, PluginHostContext context)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            if (Instance == null)
            {
                lock (_lockObject)
                {
                    if (Instance == null)
                        Instance = GetConfiguredNicknameResolverInstance(settings, context);
                }
            }
        }

        #endregion

        #region Non Public Methods

        private static INicknameResolver GetConfiguredNicknameResolverInstance(TMSPSCorePluginSettings settings, PluginHostContext context)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            INicknameResolver provider = Instancer.GetInstanceOfInterface<INicknameResolver>(settings.NicknameResolverAssemblyLocation, settings.NicknameResolverClass);
            provider.Init(context, settings.NicknameResolverConfigElement);

            return provider;
        }

        #endregion
    }
}
