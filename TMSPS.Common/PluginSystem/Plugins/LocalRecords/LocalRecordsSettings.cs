﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Configuration;
using TMSPS.Core.Common;
using TMSPS.Core.Logging;
using TMSPS.Core.PluginSystem.Configuration;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class LocalRecordsSettings : SettingsBase
	{
	    #region Constants

	    public const uint MAX_RECORDS_TO_REPORT = 50;

	    #endregion

	    #region Properties

	    public string ProviderAssemblyLocation { get; private set; }
	    public string ProviderClass { get; private set; }
	    public string ProviderParameter { get; private set; }
        public uint MaxRecordsToReport { get; private set; }

	    #endregion

	    #region Constructors

	    private LocalRecordsSettings()
	    {
			
	    }

	    #endregion

	    #region Public Methods

	    public static LocalRecordsSettings ReadFromFile(string xmlConfigurationFile)
	    {
	        LocalRecordsSettings result = new LocalRecordsSettings();
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

	        result.ProviderAssemblyLocation = assemblyElement.Value.Trim();

	        XElement providerElement = providerSettingsElement.Element("Provider");

	        if (providerElement == null)
	            throw new ConfigurationErrorsException("Could not find Provider node in file: " + xmlConfigurationFile);

	        if (providerElement.Value.IsNullOrTimmedEmpty())
	            throw new ConfigurationErrorsException("Provider node is empty in file: " + xmlConfigurationFile);

	        result.ProviderClass = providerElement.Value.Trim();

	        XElement parameterElement = providerSettingsElement.Element("Parameter");

	        if (parameterElement == null)
	            throw new ConfigurationErrorsException("Could not find Parameter node in file: " + xmlConfigurationFile);

	        result.ProviderParameter = parameterElement.Value.Trim();
            result.MaxRecordsToReport = ReadConfigUInt(configDocument.Root, "MaxRecordsToReport", MAX_RECORDS_TO_REPORT, xmlConfigurationFile);
	        result.Plugins = PluginConfigEntryCollection.ReadFromXElement(configDocument.Root.Element("Plugins"));

	        return result;
	    }

	    public List<ILocalRecordsPluginPlugin> GetPlugins(IUILogger logger)
	    {
	        List<ILocalRecordsPluginPlugin> result = new List<ILocalRecordsPluginPlugin>();

	        foreach (PluginConfigEntry pluginConfigEntry in Plugins.GetEnabledPlugins())
	        {
	            if (logger != null)
	                logger.Debug(string.Format("Instantiating ILocalRecordsPluginPlugin {0}", pluginConfigEntry.PluginClass));

	            result.Add(Instancer.GetInstanceOfInterface<ILocalRecordsPluginPlugin>(pluginConfigEntry.AssemblyName, pluginConfigEntry.PluginClass));
	        }

	        return result;
	    }

       

	    #endregion
	}
}