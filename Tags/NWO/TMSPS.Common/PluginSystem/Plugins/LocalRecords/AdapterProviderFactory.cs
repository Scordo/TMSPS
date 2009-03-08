using System;
using System.Reflection;

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

			string assemblyLocation = settings.ProviderAssemblyLocation;

			if (!assemblyLocation.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
				assemblyLocation += ".dll";

    		Assembly assembly;

    		try
    		{
    			assembly = Assembly.LoadFrom(assemblyLocation);
    		}
    		catch (Exception ex)
    		{
    			throw new ArgumentException("Could not load Assembly " + assemblyLocation, ex);
    		}

    		object providerInstance;

    		try
    		{
				providerInstance = assembly.CreateInstance(settings.ProviderClass);
    		}
    		catch (Exception ex)
    		{
				throw new ArgumentException("Could not create instance of " + settings.ProviderClass, ex);
    		}

    		if (providerInstance == null)
				throw new ArgumentException("Could not create instance of " + settings.ProviderClass);

    		if (!(providerInstance is IAdapterProvider))
    			throw new ArgumentException(string.Format("Class '{0}' does not implement IAdapterProvider.", providerInstance.GetType().FullName));

    		IAdapterProvider provider = (IAdapterProvider) providerInstance;
    		provider.Init(settings.ProviderParameter);

    		return provider;
    	}

    	#endregion
    }
}