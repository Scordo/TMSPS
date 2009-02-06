using System;
using System.Configuration;
using System.Reflection;
using System.Xml.Linq;

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

		public static IAdapterProvider GetAdapterProvider(string xmlConfigurationFile)
		{
			if (xmlConfigurationFile == null)
				throw new ArgumentNullException("xmlConfigurationFile");

			if (_instance == null)
			{
				lock (_padlock)
				{
					if (_instance == null)
						_instance = GetConfiguredProviderInstance(xmlConfigurationFile);
				}
			}

			return _instance;
		}

    	#endregion


    	#region Non Public Methods

		private static IAdapterProvider GetConfiguredProviderInstance(string xmlConfigurationFile)
    	{
			XDocument configDocument = XDocument.Load(xmlConfigurationFile);

			if (configDocument.Root == null)
				throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

			XElement providerSettingsElement = configDocument.Root.Element("ProviderSettings");

			if (providerSettingsElement == null)
				throw new ConfigurationErrorsException("Could not find ProviderSettings node in file: " + xmlConfigurationFile);

			XElement assemblyElement = providerSettingsElement.Element("Assembly");

			if (assemblyElement == null)
				throw new ConfigurationErrorsException("Could not find Assembly node in file: " + xmlConfigurationFile);

			if (assemblyElement.Value.IsNullOrTimmedEmpty())
				throw new ConfigurationErrorsException("Assembly node is empty in file: " + xmlConfigurationFile);

			XElement providerElement = providerSettingsElement.Element("Provider");

			if (providerElement == null)
				throw new ConfigurationErrorsException("Could not find Provider node in file: " + xmlConfigurationFile);

			if (providerElement.Value.IsNullOrTimmedEmpty())
				throw new ConfigurationErrorsException("Provider node is empty in file: " + xmlConfigurationFile);

			XElement parameterElement = providerSettingsElement.Element("Parameter");

			if (parameterElement == null)
				throw new ConfigurationErrorsException("Could not find Parameter node in file: " + xmlConfigurationFile);


			string assemblyLocation = assemblyElement.Value.Trim();
			string adapterProviderClass = providerElement.Value.Trim();
			string parameter = parameterElement.Value.Trim();

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