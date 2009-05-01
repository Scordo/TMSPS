using System;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    /// <summary>
    /// This class is used to get the adapter provider which gives us access to all the other adapters
    /// </summary>
    public static class AdapterProviderFactory
    {
        #region Non Public Members

        static IAdapterProvider _instance;
        static readonly object _padlock = new object();

        #endregion

    	#region Public Methods

		public static IAdapterProvider GetAdapterProvider(LocalRecordsSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			if (_instance == null)
			{
				lock (_padlock)
				{
					if (_instance == null)
						_instance = GetConfiguredProviderInstance(settings);
				}
			}

			return _instance;
		}

    	#endregion


    	#region Non Public Methods

		private static IAdapterProvider GetConfiguredProviderInstance(LocalRecordsSettings settings)
    	{
			if (settings == null)
				throw new ArgumentNullException("settings");

            IAdapterProvider provider = Instancer.GetInstanceOfInterface<IAdapterProvider>(settings.ProviderAssemblyLocation, settings.ProviderClass); 
            provider.Init(settings.ProviderParameter);

    		return provider;
    	}

    	#endregion
    }
}