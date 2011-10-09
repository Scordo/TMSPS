using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Configuration;
using TMSPS.Core.Common;
using TMSPS.Core.Logging;
using TMSPS.Core.PluginSystem.Configuration;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class LocalRecordsSettings : SettingsBase
	{
	    #region Constants

	    public const uint MAX_RECORDS_TO_REPORT = 50;
        public const string CHEATER_DELETED_MSG = "{[#ServerStyle]}> {[#MessageStyle]}Stats of cheater with login {[#HighlightStyle]}{[Login]} {[#MessageStyle]}deleted!";
        public const string CHEATER_DELETION_FAILED_MSG = "{[#ServerStyle]}> {[#MessageStyle]}Cheater with login {[#HighlightStyle]}{[Login]} {[#MessageStyle]}does not exist!";
        public const string CHEATER_BANNED_MSG = "{[#ServerStyle]}>> {[#ErrorStyle]}Cheater detected! {[#MessageStyle]}Login is {[#HighlightStyle]}{[Login]}. {[#MessageStyle]}Cheater got banned and blacklisted. All stats deleted.";

	    #endregion

	    #region Properties

	    public DatabaseType DatabaseType { get; private set; }
	    public string DatabaseConnectionString { get; private set; }
        public uint MaxRecordsToReport { get; private set; }

        public string CheaterDeletedMessage { get; private set; }
        public string CheaterDeletionFailedMessage { get; private set; }
        public string CheaterBannedMessage { get; private set; }

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

            XElement databaseSettingsElement = configDocument.Root.Element("DatabaseSettings");

	        if (databaseSettingsElement == null)
                throw new ConfigurationErrorsException("Could not find DatabaseSettings node in file: " + xmlConfigurationFile);

            XElement databaseTypeElement = databaseSettingsElement.Element("DatabaseType");

	        if (databaseTypeElement == null)
                throw new ConfigurationErrorsException("Could not find DatabaseType node in file: " + xmlConfigurationFile);

	        if (databaseTypeElement.Value.IsNullOrTimmedEmpty())
                throw new ConfigurationErrorsException("DatabaseType node is empty in file: " + xmlConfigurationFile);

            DatabaseType databaseType;

	        try
	        {
                databaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), databaseTypeElement.Value.Trim(), true);

                if (!Enum.IsDefined(typeof(DatabaseType), databaseType))
                    throw new ArgumentException();
	        }
	        catch (ArgumentException)
	        {
	            throw new ConfigurationErrorsException(string.Format("DatabaseType node has an invalid value of '{0}': {1}", databaseTypeElement.Value, xmlConfigurationFile));
	        }

            result.DatabaseType = databaseType;

            XElement databaseConnectionStringElement = databaseSettingsElement.Element("DatabaseConnectionString");

	        if (databaseConnectionStringElement == null)
                throw new ConfigurationErrorsException("Could not find DatabaseConnectionString node in file: " + xmlConfigurationFile);

	        if (databaseConnectionStringElement.Value.IsNullOrTimmedEmpty())
                throw new ConfigurationErrorsException("DatabaseConnectionString node is empty in file: " + xmlConfigurationFile);

	        result.DatabaseConnectionString = databaseConnectionStringElement.Value.Trim();

            result.MaxRecordsToReport = ReadConfigUInt(configDocument.Root, "MaxRecordsToReport", MAX_RECORDS_TO_REPORT, xmlConfigurationFile);
            result.CheaterDeletedMessage = ReadConfigString(configDocument.Root, "CheaterDeletedMessage", CHEATER_DELETED_MSG, xmlConfigurationFile);
            result.CheaterDeletionFailedMessage = ReadConfigString(configDocument.Root, "CheaterDeletionFailedMessage", CHEATER_DELETION_FAILED_MSG, xmlConfigurationFile);
            result.CheaterBannedMessage = ReadConfigString(configDocument.Root, "CheaterBannedMessage", CHEATER_BANNED_MSG, xmlConfigurationFile);
            result.Plugins = PluginConfigEntryCollection.ReadFromDirectory(Path.GetDirectoryName(xmlConfigurationFile));

	        return result;
	    }

	    public List<ILocalRecordsPluginPlugin> GetPlugins(IUILogger logger)
	    {
	        List<ILocalRecordsPluginPlugin> result = new List<ILocalRecordsPluginPlugin>();

	        foreach (PluginConfigEntry pluginConfigEntry in Plugins.GetEnabledPlugins())
	        {
	            if (logger != null)
	                logger.Debug(string.Format("Instantiating ILocalRecordsPluginPlugin {0}", pluginConfigEntry.PluginClass));

                result.Add(Instancer.GetInstanceOfInterface<ILocalRecordsPluginPlugin>(pluginConfigEntry.AssemblyName, pluginConfigEntry.PluginClass, pluginConfigEntry.PluginDirectory));
	        }

	        return result;
	    }

       

	    #endregion
	}
}