using System;
using System.Configuration;
using System.Reflection;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL;

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

        #region Properties

        public static IAdapterProvider AdapterProvider
        {
            get
            {
                if (_instance == null)
                {
                    lock (_padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = GetConfiguredProviderInstance();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Non Public Methods

        private static IAdapterProvider GetConfiguredProviderInstance()
        {
            // this will be changed later to be read from a config file ....
            string assemblyLocation = typeof(AdapterProviderFactory).Assembly.Location;
            string adapterProviderClass = typeof(AdapterProvider).FullName;
            string parameter = ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;

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
                providerInstance = assembly.CreateInstance(adapterProviderClass);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Could not create instance of " + adapterProviderClass, ex);
            }

            if (providerInstance == null)
                throw new ArgumentException("Could not create instance of " + adapterProviderClass);

            if (!(providerInstance is IAdapterProvider))
                throw new ArgumentException(string.Format("Class '{0}' does not implement IAdapterProvider.", providerInstance.GetType().FullName));

            IAdapterProvider provider = (IAdapterProvider) providerInstance;
            provider.Init(parameter);

            return provider;
        }

        #endregion
    }
}